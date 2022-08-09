import { defineStore } from "pinia";
import { backend } from "../webapi/backend";
import { useCitiesStore } from "./citiesStore";

export const useStationsStore = defineStore("stations", {
  /*
    The entries are in records shaped like below. They are stored
    in a hash table indexed by "id" (well, a JavaScript object, really)
      {
        "id": 0,
        "nameFi": "string",
        "nameSe": "string",
        "nameEn": "string",
        "addrFi": "string",
        "addrSe": "string",
        "cityId": 0,
        "capacity": 0,
        "latitude": 0,
        "longitude": 0.

        "city": {  // a record from the "cities" store
          id: 0,
          CityFi: "string",
          CitySe. "string"
        }
      }
   */
  state: () => ({
    stations: {},
    loaded: false,
    loading: false,
    errorMessage: null,
    loadStatus: "Citybike station list not yet initialized",
  }),
  getters: {
    stationCount() {
      return Object.keys(this.stations).length;
    },
  },
  actions: {
    googleMapsUrl(station) {
      /*
        Reminder on Google Maps links:
        https://www.google.com/maps/@{lat},{long},{zoom}z
        https://www.google.com/maps/@60.1635308918594,24.9145164996449,20z
      */
      return `https://www.google.com/maps/@${station.latitude},${station.longitude},20z`;
    },
    sleep(milliseconds = 400) {
      // debug helper ...
      return new Promise((r) => setTimeout(r, milliseconds));
    },
    async loadFromDb(stepDelay = 0) {
      try {
        // make sure cities are loaded first!
        this.loading = true;
        this.loadStatus = "Loading citybike stations from DB";
        await this.sleep(stepDelay);
        const cities = useCitiesStore();
        if (!cities.loaded) {
          this.loadStatus = "Loading city data from DB";
          await this.sleep(stepDelay);
          await cities.loadFromDb();
          this.loadStatus = "Loaded city data from DB";
          await this.sleep(stepDelay);
        }
        // console.log("LOAD station DB");
        // LoadingBar.start();
        this.loadStatus = "Loading station data from DB";
        await this.sleep(stepDelay);
        const response = await backend.getStationsCached();
        this.loadStatus = "Loaded station data from DB";
        await this.sleep(stepDelay);
        const raw = response.data;
        // console.log(raw);
        console.log(`Received ${raw.length} stations.`);
        // Augment the response records
        for (var s of raw) {
          s.city = cities.cities[s.cityId];
          s.depStats = { count: 0, distSum: 0, durSum: 0 }; // preallocate
          s.retStats = { count: 0, distSum: 0, durSum: 0 }; // preallocate
          s.rideRank = 0; // preallocate
          this.stations[s.id] = s;
        }
        this.errorMessage = null;
        this.loaded = true;
        this.loadStatus = `Successfully loaded ${raw.length} stations from DB`;
        await this.sleep();
      } catch (err) {
        console.log("LOAD station DB FAILED");
        console.log(err);
        this.errorMessage = err;
        this.loadStatus = "Loading station info FAILED";
        await this.sleep();
        this.loaded = false;
      } finally {
        // LoadingBar.stop();
        this.loading = false;
      }
    },
  },
});

export function statsAvgDistance(stats) {
  if (stats && stats.count) {
    return Math.round(stats.distSum / stats.count);
  }
}

export function statsAvgDuration(stats) {
  if (stats && stats.count) {
    return Math.round(stats.durSum / stats.count);
  }
}
