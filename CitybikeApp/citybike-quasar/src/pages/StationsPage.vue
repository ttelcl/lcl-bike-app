<template>
  <q-page class="q-pa-md">
    <q-breadcrumbs>
      <q-breadcrumbs-el icon="home" to="/" />
      <q-breadcrumbs-el label="Stations" icon="warehouse" />
    </q-breadcrumbs>
    <h2>{{ myName }}</h2>
  </q-page>
</template>

<script>
import { mapWritableState } from "pinia";
import { useAppstateStore } from "../stores/appstateStore";

export default {
  name: "StationsPage",
  data() {
    return {
      myName: "Citybike Station Index",
    };
  },
  computed: {
    ...mapWritableState(useAppstateStore, ["currentSection"]),
  },
  mounted() {
    this.currentSection = this.myName;
  },
};
</script>
