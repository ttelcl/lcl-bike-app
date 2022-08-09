import { defineStore } from "pinia";

export const useAppstateStore = defineStore("appstate", {
  state: () => ({
    currentSection: "Home", // Name that is merged into the top bar
    devVerbosity: 5, // not used
    manualLoadStations: false,
    showHintsInStationsList: true,
    showHintsInRidesBrowser: true,
  }),
  getters: {},
  actions: {
    changeSection(newSection) {
      this.currentSection = newSection;
    },
  },
});
