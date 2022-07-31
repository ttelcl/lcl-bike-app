import { defineStore } from "pinia";
import { backend } from "../webapi/backend";
// import { LoadingBar } from "quasar"; // no need to use explicitly, it hooks into axios

export const useCitiesStore = defineStore("cities", {
  state: () => ({
    cities: {
      0: { id: 0, CityFi: "Helsinki", CitySe: "((not loaded))" },
      1: { id: 1, CityFi: "Espoo", CitySe: "((not loaded))" },
    },
    loaded: false,
    errorMessage: null,
  }),
  getters: {},
  actions: {
    async loadFromDb() {
      try {
        // console.log("LOAD city DB");
        // LoadingBar.start();
        // await new Promise((r) => setTimeout(r, 5000)); // FAKE DELAY
        const response = await backend.getCitiesCached(4000);
        const raw = response.data;
        console.log(raw);
        for (const c of raw) {
          const c2 = {
            id: c.id,
            CityFi: c.cityFi,
            CitySe: c.citySe,
          };
          this.cities[c.id] = c2;
        }
        this.errorMessage = null;
        this.loaded = true;
      } catch (err) {
        console.log("LOAD city DB FAILED");
        console.log(err);
        this.errorMessage = err;
        this.loaded = false;
      } finally {
        // LoadingBar.stop();
      }
    },
  },
});
