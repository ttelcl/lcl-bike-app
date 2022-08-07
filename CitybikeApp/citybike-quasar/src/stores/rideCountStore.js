// This Pinia store stores a cache of counts of rides for each
// (departure station, return station) pair, as well as projections
// along both of those axes (i.e. ride count per departure station
// and ride count per return station)

import { defineStore } from "pinia";
import { backend } from "../webapi/backend";

import { useStationsStore } from "./stationsStore";

export const useRideCountStore = defineStore("ridecounts", {
  state: () => ({
    loaded: false,
    loading: false,
    errorMessage: null,
    // An unordered list caching ALL (depId, retId, count) records
    allDepRetCounts: [],
    // A mapping from departure station ID to total ride count
    allDepCounts: {},
    // A mapping from departure station ID to total ride count
    allRetCounts: {},
  }),
  getters: {},
  actions: {
    // Load all data from the DB. "reload" forces a full reload
    // Returns true if loaded, false if already loaded
    async load(reload = false) {
      if (reload || !this.loaded) {
        try {
          console.log("loading ride counts");
          this.loaded = false;
          this.loading = true;
          this.allDepRetCounts = [];
          this.allDepCounts = {};
          this.allRetCounts = {};
          this.errorMessage = "Loading ...";
          const response = await backend.getStationPairRideCounts();
          console.log("ride counts raw loaded");
          const data = response.data;
          this.allDepRetCounts = data;
          this.errorMessage = null;
          this.loaded = true;
          console.log("projecting ride counts");
          await this.projectCounts();
          console.log("ride counts fully loaded");
        } catch (err) {
          this.loaded = false;
          this.errorMessage = err;
        } finally {
          this.loading = false;
        }
        return true;
      } else {
        return false;
      }
    },
    // Rebuilds the allDepCounts and allRetCounts caches.
    // Normally called automatically by load()
    async projectCounts() {
      const stationsStore = useStationsStore();
      if (!stationsStore.loaded) {
        await stationsStore.loadFromDb();
      }
      this.allDepCounts = {};
      this.allRetCounts = {};
      // Project the counts along both departure and return axes:
      var adc = {};
      var arc = {};
      for (const stationId of Object.keys(stationsStore.stations)) {
        adc[stationId] = 0;
        arc[stationId] = 0;
      }
      for (const r of this.allDepRetCounts) {
        var v = r.count || 0;
        const did = r.depId;
        adc[did] = (adc[did] || 0) + v;
        const rid = r.retId;
        arc[rid] = (arc[rid] || 0) + v;
      }
      this.allDepCounts = adc;
      this.allRetCounts = arc;
      // Copy into station records:
      for (const station of Object.values(stationsStore.stations)) {
        station.depCount = adc[station.id] || 0;
        station.retCount = arc[station.id] || 0;
      }
      // Calculate ranks
      const ranks = new Array(stationsStore.stationCount);
      let i = 0;
      for (const station of Object.values(stationsStore.stations)) {
        ranks[i++] = {
          id: station.id,
          rides: station.depCount + station.retCount,
        };
      }
      ranks.sort((a, b) => b.rides - a.rides);
      for (i = 0; i < ranks.length; i++) {
        stationsStore.stations[ranks[i].id].rideRank = i + 1;
      }
    },
  },
});
