<template>
  <q-page class="q-pa-md">
    <q-breadcrumbs>
      <q-breadcrumbs-el label="Home" icon="home" />
    </q-breadcrumbs>
    <div class="design-note-outer">
      <div class="design-note-inner">
        <p>
          This page is the home / landing page of the SPA. Work in progress!
        </p>
        <p>
          Boxes like this present design notes (and are not part of the actual
          application).
        </p>
      </div>
    </div>
    <h2>Home</h2>
    <p>Home, sweet home</p>
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
