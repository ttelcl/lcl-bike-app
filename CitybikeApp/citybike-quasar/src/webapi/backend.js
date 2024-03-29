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

  // DEPRECATED
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

  // DEPRECATED
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

  async getRidesCount2(
    {
      t0 = null,
      t1 = null,
      depSid = null,
      retSid = null,
      distMin = null,
      distMax = null,
      secMin = null,
      secMax = null,
    } = {},
    { timeOut = 5000 } = {}
  ) {
    var params = this.makeRideQueryParams({
      t0,
      t1,
      depSid,
      retSid,
      distMin,
      distMax,
      secMin,
      secMax,
    });
    return await api.get("/api/citybike/ridescount2", {
      timeout: timeOut,
      params,
    });
  },

  async getRidesPage2(
    { offset = 0, pageSize = 15 } = {},
    {
      t0 = null,
      t1 = null,
      depSid = null,
      retSid = null,
      distMin = null,
      distMax = null,
      secMin = null,
      secMax = null,
    } = {},
    { timeOut = 5000 } = {}
  ) {
    if (typeof offset != "number" || offset < 0) {
      console.log("Invalid 'offset' argument type; using value '0'");
      offset = 0;
    }
    if (typeof pageSize != "number" || pageSize < 1) {
      console.log("Invalid 'pageSize' argument type; using value '15'");
      pageSize = 15;
    }
    var params = this.makeRideQueryParams({
      t0,
      t1,
      depSid,
      retSid,
      distMin,
      distMax,
      secMin,
      secMax,
    });
    // console.log("Executing query: " + JSON.stringify(params));
    params.offset = offset;
    params.pageSize = pageSize;
    return await api.get("/api/citybike/ridespage2", {
      timeout: timeOut,
      params,
    });
  },

  async getStationDayDepartureRideCounts(timeOut = 10000) {
    return await api.get("/api/citybike/stationdaydepstats", {
      timeout: timeOut,
    });
  },

  // Returns the raw axios response from /api/citybike/stationpaircounts
  async getStationPairRideStats(timeOut = 10000) {
    return await api.get("/api/citybike/stationpairstats", {
      timeout: timeOut,
    });
  },

  // Internal helper for validation and URL query parameter selector
  makeRideQueryParams({
    t0 = null,
    t1 = null,
    depSid = null,
    retSid = null,
    distMin = null,
    distMax = null,
    secMin = null,
    secMax = null,
  }) {
    var params = {};
    if (typeof t0 == "string" && t0.match(/^\d{4}-\d{2}-\d{2}$/)) {
      params.t0 = t0;
    }
    if (typeof t1 == "string" && t1.match(/^\d{4}-\d{2}-\d{2}$/)) {
      params.t1 = t1;
    }
    if (Number.isFinite(depSid) && depSid > 0) {
      params.depid = depSid;
    }
    if (Number.isFinite(retSid) && retSid > 0) {
      params.retid = retSid;
    }
    if (Number.isFinite(distMin) && distMin > 0) {
      params.distMin = distMin;
    }
    if (Number.isFinite(distMax) && distMax > 0) {
      params.distMax = distMax;
    }
    if (Number.isFinite(secMin) && secMin > 0) {
      params.secMin = secMin;
    }
    if (Number.isFinite(secMax) && secMax > 0) {
      params.secMax = secMax;
    }
    return params;
  },
};
