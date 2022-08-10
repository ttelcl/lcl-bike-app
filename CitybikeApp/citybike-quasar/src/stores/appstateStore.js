import { defineStore } from "pinia";

export const useAppstateStore = defineStore("appstate", {
  state: () => ({
    currentSection: "Home", // Name that is merged into the top bar
    devVerbosity: 5, // not used
    manualLoadStations: false,
    showHintsInStationsList: true,
    showHintsInRidesBrowser: true,
    /*
      Language (FI, SE, EN) used for name and address display.
      Used in multiple places - this state is shared.
     */
    nameLanguage: "FI",
  }),
  getters: {},
  actions: {
    changeSection(newSection) {
      this.currentSection = newSection;
    },
    getStationName(station) {
      const lang = this.nameLanguage;
      return lang == "SE"
        ? station.nameSe
        : lang == "EN"
        ? station.nameEn
        : station.nameFi;
    },
    getStationAddress(station) {
      const lang = this.nameLanguage;
      return lang == "SE" ? station.addrSe : station.addrFi; // Used for both FI and EN !
    },
    getStationCity(station) {
      const lang = this.nameLanguage;
      return lang == "SE" ? station.city.CitySe : station.city.CityFi; // Used for both FI and EN !
    },
  },
});
