import { defineStore } from "pinia";

export const useAppstateStore = defineStore("appstate", {
  state: () => ({
    currentSection: "Home",
  }),
  getters: {},
  actions: {
    changeSection(newSection) {
      this.currentSection = newSection;
    },
  },
});
