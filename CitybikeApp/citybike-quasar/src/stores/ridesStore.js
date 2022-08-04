// This pinia store mostly acts as API for ride data access

import { defineStore } from "pinia";
import { backend } from "../webapi/backend";
import { utilities } from "../webapi/utilities";
import { useStationsStore } from "./stationsStore";

export const useRidesStore = defineStore("rides", {
  state: () => ({
    nextRideId: Date.now() * 100,
    lastError: null,
    loaded: false, // whether the global bounds have been loaded
    loading: false,
    allRidesCount: 0,
    firstRideStart: new Date("2021-05-01T00:00:00"), // best guess placeholder until loaded!
    lastRideStart: new Date("2021-07-31T23:59:59"), // best guess placeholder until loaded!

    currentPagination: {
      page: 1,
      rowsPerPage: 15,
      sortBy: undefined,
      descending: false,
      rowsNumber: 150, // not-undefined triggers server side pagination behaviour in <q-table>
      query: {}, // our own query info
    },
    currentPaginationInitialized: false,
    currentPageRows: [],
  }),
  getters: {
    firstRideDateString() {
      return utilities.toIsoLikeDateString(this.firstRideStart);
    },
    lastRideDateString() {
      return utilities.toIsoLikeDateString(this.lastRideStart);
    },
    firstRideString() {
      return utilities.toIsoString(this.firstRideStart);
    },
    lastRideString() {
      return utilities.toIsoString(this.lastRideStart);
    },
  },
  actions: {
    // Create a new ride query state object
    newRideQuery(pageSize = 15, t0 = null, t1 = null) {
      if (isNaN(pageSize) || pageSize < 1) {
        throw "pageSize must be a number >= 1";
      }
      if (
        t0 !== null &&
        (typeof t0 != "string" || !t0.match(/^\d{4}-\d{2}-\d{2}$/))
      ) {
        throw "t0 must be a null or a string of the form 'yyyy-mm-dd'";
      }
      if (
        t1 !== null &&
        (typeof t1 != "string" || !t1.match(/^\d{4}-\d{2}-\d{2}$/))
      ) {
        throw "t1 must be a null or a string of the form 'yyyy-mm-dd'";
      }
      return {
        offset: 0,
        pageSize,
        t0,
        t1,
      };
    },

    async getRidesCount(rideQuery) {
      const response = await backend.getRidesCount(rideQuery.t0, rideQuery.t1);
      return response.data;
    },

    cheapGuid() {
      return this.nextRideId++;
    },

    reshapeRide(rawRide, stations) {
      // Reshapes a ride record received from the backend:
      // * inject station details
      // * inject an id if not present already (backend type "RideBase" instead of "Ride")
      return {
        id: rawRide.id || this.cheapGuid(),
        depTime: new Date(rawRide.depTime),
        retTime: new Date(rawRide.retTime),
        depStationId: rawRide.depStationId,
        retStationId: rawRide.retStationId,
        distance: rawRide.distance,
        duration: rawRide.duration,
        depStation: stations.stations[rawRide.depStationId],
        retStation: stations.stations[rawRide.retStationId],
      };
    },

    // Set up the currentPagination for a new query, starting at page 1 with
    // new search parameters
    async initTable(
      loadFirstPage = false,
      pageSize = 15,
      page = 1,
      t0 = null,
      t1 = null
    ) {
      this.currentPaginationInitialized = false;
      this.currentPageRows = [];
      await this.reload(false); // make sure the basics are present
      var q = this.newRideQuery(pageSize, t0, t1);
      q.offset = (page - 1) * pageSize;
      var ridesCount =
        t0 || t1 ? await this.getRidesCount(q) : this.allRidesCount;
      this.currentPagination = {
        page,
        rowsPerPage: pageSize,
        sortBy: undefined,
        descending: false,
        rowsNumber: ridesCount, // not-undefined triggers server side pagination behaviour in <q-table>
        query: q, // our own query info
      };
      this.currentPaginationInitialized = true;
      if (loadFirstPage) {
        await this.updateTablePage({
          pagination: this.currentPagination,
          filter: undefined,
        });
      }
    },

    async updateTablePage(props) {
      // "request" callback from table for serverside pagination
      const { page, rowsPerPage, sortBy, descending } = props.pagination;
      this.loading = true;
      console.log(`updatetablePage(${page}, ${rowsPerPage})`);
      try {
        // first sync q-table's pagination with our own query object
        this.currentPagination.query.offset = (page - 1) * rowsPerPage;
        this.currentPagination.query.pageSize = rowsPerPage;
        // console.log(this.currentPagination.query);
        const serverData = await this.getRidesPage(
          this.currentPagination.query
        );
        this.currentPageRows = serverData;
        // console.log("New Content = ");
        // console.log(this.currentPageRows);
        this.currentPagination.page = page;
        this.currentPagination.rowsPerPage = rowsPerPage;
        this.currentPagination.sortBy = sortBy; // ignored by this function!
        this.currentPagination.descending = descending; // ignored
      } finally {
        this.loading = false;
      }
    },

    async getRidesPage(rideQuery) {
      const stations = useStationsStore();
      if (!stations.loaded) {
        console.log("Triggering Stations data Loading from Rides Query!");
        await stations.loadFromDb();
      }
      const response = await backend.getRidesPage(
        rideQuery.offset,
        rideQuery.t0,
        rideQuery.t1,
        rideQuery.pageSize
      );
      const raw = response.data;
      // console.log(raw);
      var l = [];
      for (const r of raw) {
        l.push(this.reshapeRide(r, stations));
      }
      // console.log(l);
      return l;
    },

    async reload(force = true) {
      try {
        if (force || !this.loaded) {
          this.loaded = false;
          const response = await backend.getRidesCount();
          this.allRidesCount = response.data;
          const response2 = await backend.getTimeRange();
          if (response2.status == 204) {
            this.lastError = "No ride data available!";
            this.loaded = false; // keep at false!
          } else {
            this.firstRideStart = new Date(response2.data.startTime);
            this.lastRideStart = new Date(response2.data.endTime);
            // console.log(
            //   `Time range is ${this.firstRideStart} to ${this.lastRideStart}`
            // );
            // console.log(
            //   `Date range is ${this.firstRideDateString} to ${this.lastRideDateString}`
            // );
            this.loaded = true;
          }
        }
      } catch (err) {
        this.lastError = err;
        this.loaded = false;
      } finally {
        this.loading = false;
      }
    },
  },
});
