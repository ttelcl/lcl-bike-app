<template>
  <q-page class="q-pa-md">
    <q-breadcrumbs>
      <q-breadcrumbs-el icon="home" to="/" />
      <q-breadcrumbs-el label="Stations" icon="warehouse" />
    </q-breadcrumbs>
    <h2>{{ myName }}</h2>
    <div>
      <p>Loaded {{ stationsStore.stationCount }} stations</p>
    </div>
    <div class="q-pa-md">
      <p v-if="stationsStore.loaded" class="text-green">Loaded from DB</p>
      <p v-else class="text-orange">Not yet loaded from DB</p>
      <q-btn color="purple" @click="reload">(Re)load</q-btn>
      <p v-if="stationsStore.errorMessage" class="text-red">
        Load error: {{ stationsStore.errorMessage }}
      </p>
    </div>
  </q-page>
</template>

<script>
import { useAppstateStore } from "../stores/appstateStore";
import { useCitiesStore } from "../stores/citiesStore";
import { useStationsStore } from "src/stores/stationsStore";
import DesignNote from "components/DesignNote.vue";

export default {
  name: "StationsPage",
  setup() {
    const appstateStore = useAppstateStore();
    const citiesStore = useCitiesStore();
    const stationsStore = useStationsStore();
    return { appstateStore, citiesStore, stationsStore };
  },
  data() {
    return {
      myName: "Citybike Station Index",
    };
  },
  computed: {
    citiesMap() {
      return this.citiesStore.cities;
    },
    citiesList() {
      return Object.values(this.citiesStore.cities);
    },
    stationsMap() {
      return this.stationsStore.stations;
    },
    stationsList() {
      return Object.values(this.stationsStore.stations);
    },
  },
  methods: {
    async reload() {
      await this.stationsStore.loadFromDb();
    },
  },
  mounted() {
    this.appstateStore.currentSection = this.myName;
  },
};
</script>
