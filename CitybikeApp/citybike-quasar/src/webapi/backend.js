import { api } from "boot/axios";

/*
 * This module defines the backend API methods in a "JavaScripty" way.
 */

export const backend = {
  async getApiTestData(timeOut = 2000) {
    return await api.get("/api/scratch/dummy", { timeout: timeOut });
  },

  async getCitiesCached(timeOut = 4000) {
    return await api.get("/api/citybike/cities", { timeout: timeOut });
  },
};
