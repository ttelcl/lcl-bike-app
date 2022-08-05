<template>
  <q-page class="q-pa-md">
    <q-breadcrumbs>
      <q-breadcrumbs-el icon="home" to="/" />
      <q-breadcrumbs-el label="Developer Extras" icon="warehouse" />
    </q-breadcrumbs>
    <h2 class="q-my-md">{{ myName }}</h2>
    <div class="simple-text">
      <h4>Some links to assist with development</h4>
      <p>
        For these to work, the backend must be reachable. During development,
        that means both frontend server (http://localhost:9000/) and backend
        server (https://localhost:7185/) must be running
      </p>
      <ul>
        <li>
          <a href="/swagger/index.html">Swagger (backend Web API docs)</a>
        </li>
        <li>
          <a href="/home/">Backend Home</a>
        </li>
        <li>
          <a href="/api/scratch/dummy"
            >Direct API call example (without hitting DB)</a
          >
        </li>
        <li>
          <a href="/api/citybike/cities-raw"
            >Direct API call example (hitting DB)</a
          >
        </li>
      </ul>
      <hr />
      <div class="row">
        <div class="col-3">
          <q-btn @click="getData" color="primary">Async API Test</q-btn>
        </div>
        <div class="col-9">
          <div>Returned data:</div>
          <pre class="text-primary"
            >{{ returnedData }}
          </pre>
          <p>Backend Server Time is: {{ serverTime ? serverTime : "?" }}</p>
          <div>
            Error:
            <span v-if="errorMessage" class="text-red">{{ errorMessage }}</span>
            <span v-else class="text-positive">OK</span>
          </div>
        </div>
      </div>
      <hr />
      <div class="row q-my-sm">
        <h6 class="q-my-none">
          Testing how performant /api/stationdaydepstats is.
        </h6>
      </div>
      <div class="row q-my-xs">
        <p class="q-my-none">(expected to return almost 40000 records)</p>
      </div>
      <div class="row q-my-md">
        <div class="col-3">
          <q-btn color="primary" @click="getStats">Load Big Data</q-btn>
        </div>
        <div class="col">
          <div>"stationdaydepstats" return size: {{ statRecordCount }}</div>
          <div>"station42Departures": {{ station42Departures }}</div>
        </div>
      </div>
    </div>
  </q-page>
</template>

<script>
import { mapWritableState } from "pinia";
import { useAppstateStore } from "../stores/appstateStore";
import { backend } from "../webapi/backend";

export default {
  name: "DevExtrasPage",
  data() {
    return {
      myName: "Development Extras",
      backendData: null,
      errorMessage: "",
      statRecords: [], // TEMPORARY
      station42Departures: 0,
    };
  },
  computed: {
    ...mapWritableState(useAppstateStore, ["currentSection"]),
    returnedData() {
      return JSON.stringify(this.backendData, null, 4);
    },
    serverTime() {
      return this.backendData ? this.backendData.ServerTime : null;
    },
    statRecordCount() {
      return this.statRecords.length;
    },
  },
  methods: {
    async getData() {
      console.log("getData(1)");
      try {
        this.backendData = null;
        this.errorMessage = "...";
        console.log("getData(2): GET");
        // const response = await api.get("/api/scratch/dummy", {
        //   timeout: 2000,
        // });
        const response = await backend.getApiTestData(2000);
        console.log("getData(3): RESPONSE");
        this.backendData = response.data;
        this.errorMessage = "";
      } catch (err) {
        console.log("getData(4): ERROR");
        console.log(err);
        this.backendData = null;
        this.errorMessage = err;
      }
    },
    async getStats() {
      // This is a test to see if the 40000 records returned by
      // /api/stationdaydepstats make anything choke (it does choke Swagger!)
      // Conclusion: it works just fine!
      try {
        this.statRecords = [];
        this.station42Departures = 0;
        this.errorMessage = "...";
        const response = await backend.getStationDayDepartureRideCounts();
        const data = response.data;
        this.statRecords = data;
        this.errorMessage = "";
        var sum = 0;
        for (const r of data) {
          if (r.stationId == 42) {
            sum += r.count;
          }
        }
        this.station42Departures = sum;
      } catch (err) {
        this.errorMessage = err;
      }
    },
  },
  mounted() {
    this.currentSection = this.myName;
  },
};
</script>
