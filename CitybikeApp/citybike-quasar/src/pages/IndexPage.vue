<template>
  <q-page class="q-pa-md">
    <h2>Home</h2>
    <p>sweet home</p>
  </q-page>
</template>

<script>
import { mapWritableState } from "pinia";
import { useAppstateStore } from "../stores/appstateStore";

export default {
  name: "IndexPage",
  data() {
    return {
      myName: "Home",
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
