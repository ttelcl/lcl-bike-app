// This pinia store stores the pagination state of the
// "Citybike Station Index" page so the view can restore when returning

import { defineStore } from "pinia";
import { useStationsStore } from "./stationsStore";

export const useStationsViewStore = defineStore("stationsview", {
  state: () => ({
    pagination: {
      page: 1,
      rowsPerPage: 15,
      sortBy: "rank",
      descending: false,
    },
    searchText: null,
    lastSearch: null, // used in distinguishing a new search from a state restore
  }),
  actions: {
    reset() {
      this.pagination.page = 1;
      this.pagination.rowsPerPage = 15;
      this.pagination.sortBy = "rank";
      this.pagination.descending = false;
      this.searchText = null;
    },
  },
});
