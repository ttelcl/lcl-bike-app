import { defineStore } from "pinia";

export const useCitiesStore = defineStore("cities", {
  state: () => ({
    cities: {
      0: { id: 0, CityFi: "Helsinki", CitySe: "((not loaded))" },
      1: { id: 1, CityFi: "Espoo", CitySe: "((not loaded))" },
    },
  }),
  getters: {},
  actions: {},
});
