<template>
  <q-page class="q-pa-md">
    <q-breadcrumbs>
      <q-breadcrumbs-el icon="home" to="/" />
      <q-breadcrumbs-el label="Rides" icon="directions_bike" />
    </q-breadcrumbs>
    <h2 class="q-my-md">{{ myName }}</h2>
    <ul>
      <li>All Rides Count: {{ allRideCount }}</li>
      <li><q-btn label="fetch" @click="getAllRidesCount" /></li>
    </ul>
  </q-page>
</template>

<script>
import { useAppstateStore } from "../stores/appstateStore";
import { useRidesStore } from "../stores/ridesStore";

export default {
  name: "RidesPage",
  setup() {
    const appstateStore = useAppstateStore();
    const ridesStore = useRidesStore();
    return { appstateStore, ridesStore };
  },
  data() {
    return {
      myName: "Rides Browser",
      rideQuery: this.ridesStore.newRideQuery(15, null, null),
      allRideCount: 0,
    };
  },
  computed: {},
  methods: {
    async getAllRidesCount() {
      const n = await this.ridesStore.getRidesCount(this.rideQuery);
      this.allRideCount = n;
    },
  },
  mounted() {
    this.appstateStore.currentSection = this.myName;
  },
};
</script>
