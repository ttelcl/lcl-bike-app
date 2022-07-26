<template>
  <q-layout view="hHh Lpr lFf">
    <q-header elevated>
      <q-toolbar>
        <q-btn
          flat
          dense
          round
          icon="menu"
          aria-label="Menu"
          @click="toggleLeftDrawer"
        />

        <q-toolbar-title> Citybike App - {{ currentSection }}</q-toolbar-title>

        <!-- <div>Quasar v{{ $q.version }}</div> -->
      </q-toolbar>
    </q-header>

    <q-drawer
      v-model="leftDrawerOpen"
      bordered
      :mini="miniState"
      @mouseover="miniState = false"
      @mouseout="miniState = true"
      mini-to-overlay
      :width="200"
      :breakpoint="500"
    >
      <q-list>
        <AppSectionHeader
          v-for="appSection in sections"
          :key="appSection.label"
          v-bind="appSection"
        />
      </q-list>
    </q-drawer>

    <q-page-container>
      <router-view />
    </q-page-container>
  </q-layout>
</template>

<script>
import { defineComponent } from "vue";
import { mapState } from "pinia";
import { useAppstateStore } from "../stores/appstateStore";
import AppSectionHeader from "components/AppSectionHeader.vue";

const sectionsList = [
  {
    label: "Home",
    icon: "home",
    target: "/",
  },
  {
    label: "Citybike Stations",
    icon: "map",
    target: "stations",
  },
  {
    label: "Rides",
    icon: "directions_bike",
    target: "rides",
  },
  {
    label: "Cities",
    icon: "location_city",
    target: "cities",
  },
];

export default defineComponent({
  name: "MainLayout",

  components: {
    AppSectionHeader,
  },

  data() {
    return {
      sections: sectionsList,
      leftDrawerOpen: true,
      miniState: true,
    };
  },

  methods: {
    toggleLeftDrawer() {
      this.leftDrawerOpen.value = !this.leftDrawerOpen.value;
    },
  },

  computed: {
    ...mapState(useAppstateStore, ["currentSection"]),
  },
});
</script>
