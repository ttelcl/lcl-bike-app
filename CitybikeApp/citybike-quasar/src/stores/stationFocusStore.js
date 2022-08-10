/*
  This pinia store caches information for the one-station page, caching
  interconnection stats to other stations
*/

import { defineStore } from "pinia";
import { useStationsStore } from "./stationsStore";
import { useRideCountStore } from "./rideCountStore";

export const useStationFocusStore = defineStore("stationfocus", {
  state: () => ({
    station: null,
    // A map from remote station ID to a record containing:
    // - id = that remote ID
    // - incoming: the stats record for rides from remoteId to station.id
    // - outgoing: the stats record for rides from station.id to remoteId
    // The stats records are initialized for all station pairs, so they
    // exist even where there are no rides between the two stations.
    linkStats: {},
    // The values of linkStats in an array, initially sorted by
    // x.rank, with entries with no rides removed.
    linkStatsList: [],
    pagination: {
      page: 1,
      rowsPerPage: 10,
      sortBy: undefined,
      descending: false,
    },
  }),
  getters: {
    stationId() {
      return this.station ? this.station.id : undefined;
    },
  },
  actions: {
    // re-initialize this store's content and ensure the required
    // info has been loaded in stationsStore and rideCountStore
    async reset(stationId) {
      const stationsStore = useStationsStore();
      const rideCountStore = useRideCountStore();
      await stationsStore.loadIfNotDoneSoYet();
      await rideCountStore.load(false);
      this.resetCore(stationId);
    },
    // Re-initialize this store's content. Does not ensure that
    // the dependent stores are initialized.
    resetCore(stationId) {
      const stationsStore = useStationsStore();
      const rideCountStore = useRideCountStore();
      if (!this.station || this.station.id != stationId) {
        console.log(
          "Loading station data for station " + JSON.stringify(stationId)
        );
        this.station = null; // unload first
        this.linkStats = {};
        this.linkStatsList = [];
        const station = stationsStore.stations[stationId];
        if (station) {
          const allDepRetStats = rideCountStore.allDepRetStats;
          const linkStats = {};
          for (const remoteStation of Object.values(stationsStore.stations)) {
            // Initialize the incoming and outgoing records. Most will be
            // replaced in the next step.
            const remoteId = remoteStation.id;
            linkStats[remoteId] = {
              id: remoteId,
              station: remoteStation,
              incoming: {
                depId: remoteId,
                retId: stationId,
                count: 0,
                distSum: 0,
                durSum: 0,
              },
              outgoing: {
                depId: stationId,
                retId: remoteId,
                count: 0,
                distSum: 0,
                durSum: 0,
              },
              count: 0, // = incoming.count + outgoing.count
              rank: 0, // reverse order of count
              rankIn: 0,
              rankOut: 0,
            };
          }
          for (const stats of allDepRetStats) {
            if (stats.retId == stationId) {
              linkStats[stats.depId].incoming = stats;
            }
            if (stats.depId == stationId) {
              linkStats[stats.retId].outgoing = stats;
            }
          }
          const linkStatsList = Object.values(linkStats).filter(
            (e) => e.incoming.count > 0 || e.outgoing.count > 0
          );

          for (let i = 0; i < linkStatsList.length; i++) {
            const l = linkStatsList[i];
            l.count = l.incoming.count + l.outgoing.count;
          }

          // Calculate ranks
          linkStatsList.sort((a, b) => b.incoming.count - a.incoming.count);
          for (let i = 0; i < linkStatsList.length; i++) {
            linkStatsList[i].rankIn = i + 1;
          }

          linkStatsList.sort((a, b) => b.outgoing.count - a.outgoing.count);
          for (let i = 0; i < linkStatsList.length; i++) {
            linkStatsList[i].rankOut = i + 1;
          }

          // Make sure to sort for total ride count ranking last (this
          // will be the initial view)
          linkStatsList.sort((a, b) => b.count - a.count);
          for (let i = 0; i < linkStatsList.length; i++) {
            linkStatsList[i].rank = i + 1;
          }

          this.station = station;
          this.linkStats = linkStats;
          this.linkStatsList = linkStatsList;

          this.pagination.sortBy = "rank";
          this.pagination.descending = false;
          this.pagination.page = 1;

          // console.log(
          //   "Top connecting station: " + JSON.stringify(linkStatsList[0])
          // );
          // console.log("Runner up: " + JSON.stringify(linkStatsList[1]));
        } // else: invalid ID: skip load
      } else {
        console.log(
          "Already loaded station data for station " + JSON.stringify(stationId)
        );
      }
    },
  },
});
