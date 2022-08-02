// This pinia store mostly acts as API for ride data access

import { defineStore } from "pinia";
import { backend } from "../webapi/backend";
import { utilities } from "../webapi/utilities";
import { useStationsStore } from "./stationsStore";

export const useRidesStore = defineStore("rides", {
  state: () => ({
    nextRideId: Date.now() * 1000000,
    lastError: null,
    loaded: false, // whether the global bounds have been loaded
    loading: false,
    allRideCount: 0,
    firstRideStart: Date.parse("2021-05-01T00:00:00"), // best guess placeholder until loaded!
    lastRideStart: Date.parse("2021-07-31T23:59:59"), // best guess placeholder until loaded!
  }),
  getters: {
    firstRideDateString() {
      return utilities.toIsoLikeDateString(this.firstRideStart);
    },
    lastRideDateString() {
      return utilities.toIsoLikeDateString(this.lastRideStart);
    },
  },
  actions: {
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
      // reshape and inject station details and a guid in a ride record received from the backend
      return {
        id: rawRide.id || this.cheapGuid(),
        depTime: Date.parse(rawRide.depTime),
        retTime: Date.parse(rawRide.retTime),
        depStationId: rawRide.depStationId,
        retStationId: rawRide.retStationId,
        distance: rawRide.distance,
        duration: rawRide.duration,
        depStation: stations.stations[rawRide.depStationId],
        retStation: stations.stations[rawRide.retStationId],
      };
    },

    async getRidesPage(rideQuery) {
      const stations = useStationsStore();
      if (!stations.loaded) {
        console.log("Loading Stations from Rides Query!");
        await stations.loadFromDb();
      }
      const response = await backend.getRidesPage(
        rideQuery.offset,
        rideQuery.t0,
        rideQuery.t1,
        rideQuery.pageSize
      );
      const raw = response.data;
      l = [];
      for (const r of raw) {
        l.push(this.reshapeRide(r));
      }
      return l;
    },

    async reload(force = true) {
      try {
        if (force || !this.loaded) {
          this.loaded = false;
          const response = await backend.getRidesCount();
          this.allRideCount = response.data;
          throw "Retrieving Time Range NYI in backend";
          this.loaded = true;
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
