import { defineStore } from "pinia";

export const useAppstateStore = defineStore("appstate", {
  state: () => ({
    currentSection: "Home", // Name that is merged into the top bar
    devVerbosity: 5,
    manualLoadStations: false,
  }),
  getters: {},
  actions: {
    changeSection(newSection) {
      this.currentSection = newSection;
    },
  },
});
