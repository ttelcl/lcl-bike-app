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
    errorMessage: null,
  }),
  getters: {
    stationCount() {
      return Object.keys(this.stations).length;
    },
  },
  actions: {
    async loadFromDb() {
      try {
        // make sure cities are loaded first!
        const cities = useCitiesStore();
        if (!cities.loaded) {
          await cities.loadFromDb();
        }
        // console.log("LOAD station DB");
        // LoadingBar.start();
        const response = await backend.getStationsCached();
        const raw = response.data;
        // console.log(raw);
        console.log(`Received ${raw.length} stations.`);
        for (var s of raw) {
          s.city = cities.cities[s.cityId];
          this.stations[s.id] = s;
        }
        this.errorMessage = null;
        this.loaded = true;
        // DBG
        const sample = this.stations[100];
        console.log(sample);
        console.log(JSON.stringify(sample, null, 2));
      } catch (err) {
        console.log("LOAD station DB FAILED");
        console.log(err);
        this.errorMessage = err;
        this.loaded = false;
      } finally {
        // LoadingBar.stop();
      }
    },
  },
});
