<template>
  <q-page class="q-pa-md">
    <h2>{{ myName }}</h2>
  </q-page>
</template>

<script>
import { mapWritableState } from "pinia";
import { useAppstateStore } from "../stores/appstateStore";

export default {
  name: "RidesPage",
  data() {
    return {
      myName: "Rides Browser",
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
