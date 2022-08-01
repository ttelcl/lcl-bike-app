<template>
  <q-page class="q-pa-md">
    <q-breadcrumbs>
      <q-breadcrumbs-el label="Home" icon="home" />
    </q-breadcrumbs>
    <DesignNote :expand="true">
      <p>This page is the home / landing page of the SPA. Work in progress!</p>
      <p>
        Boxes like this present design notes (and are not part of the actual
        application).
      </p>
    </DesignNote>
    <h2 class="q-my-md">{{ myName }}</h2>
    <div class="simple-text">
      <p>Home, sweet home</p>
    </div>
  </q-page>
</template>

<script>
import { mapWritableState } from "pinia";
import { useAppstateStore } from "../stores/appstateStore";
import DesignNote from "components/DesignNote.vue";

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
  components: {
    DesignNote,
  },
  mounted() {
    this.currentSection = this.myName;
  },
};
</script>
