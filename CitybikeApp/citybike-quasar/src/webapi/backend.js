import { api } from "boot/axios";

/*
 * This module defines the backend API methods in a "JavaScripty" way.
 * Most functions return a full response object, not just the data!
 */

export const backend = {
  async getApiTestData(timeOut = 2000) {
    return await api.get("/api/scratch/dummy", { timeout: timeOut });
  },

  async getCitiesCached(timeOut = 4000) {
    return await api.get("/api/citybike/cities", { timeout: timeOut });
  },

  async getStationsCached(timeOut = 5000) {
    return await api.get("/api/citybike/stations", { timeout: timeOut });
  },

  async getTimeRange(timeOut = 2000) {
    return await api.get("/api/citybike/timerange", { timeout: timeOut });
  },

  async getRidesCount(t0 = null, t1 = null, timeOut = 5000) {
    var params = {};
    if (typeof t0 == "string" && t0.match(/^\d{4}-\d{2}-\d{2}$/)) {
      params.t0 = t0;
    }
    if (typeof t1 == "string" && t1.match(/^\d{4}-\d{2}-\d{2}$/)) {
      params.t1 = t1;
    }
    return await api.get("/api/citybike/ridescount", {
      timeout: timeOut,
      params,
    });
  },

  async getRidesPage(
    offset = 0,
    t0 = null,
    t1 = null,
    pageSize = 15,
    timeOut = 5000
  ) {
    if (typeof offset != "number" || offset < 0) {
      console.log("Invalid 'offset' argument type; using value '0'");
      offset = 0;
    }
    if (typeof pageSize != "number" || pageSize < 1) {
      console.log("Invalid 'pageSize' argument type; using value '15'");
      pageSize = 15;
    }
    var params = {
      offset,
      pageSize,
    };
    if (typeof t0 == "string" && t0.match(/^\d{4}-\d{2}-\d{2}$/)) {
      params.t0 = t0;
    }
    if (typeof t1 == "string" && t1.match(/^\d{4}-\d{2}-\d{2}$/)) {
      params.t1 = t1;
    }
    return await api.get("/api/citybike/ridespage", {
      timeout: timeOut,
      params,
    });
  },
};
