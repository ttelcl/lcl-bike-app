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
    allDepRetStats: [],
    // A mapping from departure station ID to total ride count
    allDepStats: {},
    // A mapping from departure station ID to total ride count
    allRetStats: {},
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
          this.allDepRetStats = [];
          this.allDepStats = {};
          this.allRetStats = {};
          this.errorMessage = "Loading ...";
          const response = await backend.getStationPairRideStats();
          console.log("ride counts raw loaded");
          const data = response.data;
          this.allDepRetStats = data;
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
    // Rebuilds the allDepStats and allRetStats caches.
    // and injects their values in the station list
    // Normally called automatically by load()
    async projectCounts() {
      const stationsStore = useStationsStore();
      if (!stationsStore.loaded) {
        await stationsStore.loadFromDb();
      }
      this.allDepStats = {};
      this.allRetStats = {};
      // Project the counts along both departure and return axes:
      var adc = {};
      var arc = {};
      for (const stationId of Object.keys(stationsStore.stations)) {
        adc[stationId] = { count: 0, distSum: 0, durSum: 0 };
        arc[stationId] = { count: 0, distSum: 0, durSum: 0 };
      }
      for (const r of this.allDepRetStats) {
        var rd = adc[r.depId];
        rd.count += r.count || 0;
        rd.distSum += r.distSum || 0;
        rd.durSum += r.durSum || 0;
        var rr = arc[r.retId];
        rr.count += r.count || 0;
        rr.distSum += r.distSum || 0;
        rr.durSum += r.durSum || 0;
      }
      this.allDepStats = adc;
      this.allRetStats = arc;
      // Copy into station records:
      for (const station of Object.values(stationsStore.stations)) {
        station.depStats = adc[station.id] || {
          count: 0,
          distSum: 0,
          durSum: 0,
        };
        station.retStats = arc[station.id] || {
          count: 0,
          distSum: 0,
          durSum: 0,
        };
      }
      // Calculate total ride count ranks
      const ranks = new Array(stationsStore.stationCount);
      let i = 0;
      for (const station of Object.values(stationsStore.stations)) {
        ranks[i++] = {
          id: station.id,
          rides: station.depStats.count + station.retStats.count,
        };
      }
      ranks.sort((a, b) => b.rides - a.rides);
      for (i = 0; i < ranks.length; i++) {
        stationsStore.stations[ranks[i].id].rideRank = i + 1;
      }
    },
  },
});
