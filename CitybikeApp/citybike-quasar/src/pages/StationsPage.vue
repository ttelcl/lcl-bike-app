<template>
  <q-page class="q-pa-md">
    <q-breadcrumbs>
      <q-breadcrumbs-el icon="home" to="/" />
      <q-breadcrumbs-el label="Stations" icon="warehouse" />
    </q-breadcrumbs>
    <h2 class="q-my-md">{{ myName }}</h2>
    <div class="q-pa-md">
      <div>
        <q-table
          title="Citybike Stations"
          :rows="stationsList"
          :columns="columnDefs"
          row-key="id"
          separator="cell"
          dense
          bordered="true"
          v-model:pagination="pagination"
          :loading="loading"
          :rows-per-page-options="[10, 15, 20, 25, 30, 40, 50]"
        >
        </q-table>
      </div>
      <div v-if="!isLoaded" class="problem">
        Citybike Station data has not yet been successfully loaded.
        <q-btn
          color="green-10"
          @click="reload"
          padding="0 1ex"
          no-caps
          icon-right="update"
          label="(re)load"
        />
      </div>
    </div>
    <div class="dbginfo" v-if="loading || stationsStore.errorMessage">
      <h6 class="q-my-md">Debug / Develop Temporary section</h6>
      <ul>
        <li>
          Load status:
          <span class="dbgbrighter">{{ loadStatus }}</span>
        </li>
        <li>
          Load or Reload Citybike Stations info from database:
          <q-btn
            color="green-10"
            @click="reload"
            padding="0 1ex"
            no-caps
            icon="update"
          />
        </li>
        <li v-if="stationsStore.loaded" class="text-green">Loaded from DB</li>
        <li v-else class="text-orange">Not yet loaded from DB</li>
        <li v-if="stationsStore.errorMessage" class="text-red">
          Load error: {{ stationsStore.errorMessage }}
        </li>
      </ul>
    </div>
  </q-page>
</template>

<script>
import { useAppstateStore } from "../stores/appstateStore";
import { useCitiesStore } from "../stores/citiesStore";
import { useStationsStore } from "src/stores/stationsStore";
import DesignNote from "components/DesignNote.vue";

// ref https://quasar.dev/vue-components/table
const stationColumns = [
  {
    name: "id",
    label: "Id",
    field: "id",
    required: true,
    align: "right",
    // classes: "q-table--col-auto-width",
    classes: "colStyleId",
    headerClasses: "q-table--col-auto-width",
  },
  {
    name: "nameFi",
    label: "Name (FI)",
    field: "nameFi",
    align: "left",
    // classes: "q-table--col-auto-width",
    classes: "colStyleName",
    headerClasses: "q-table--col-auto-width",
  },
  {
    name: "addrFi",
    label: "Address (FI)",
    field: "addrFi",
    // classes: "q-table--col-auto-width",
    classes: "colStyleAddr",
    align: "left",
  },
  {
    name: "city",
    label: "City",
    field: (row) => row.city.CityFi,
    // classes: "q-table--col-auto-width",
    classes: "colStyleCity",
    align: "left",
  },
  {
    // virtual column to put action buttons in
    name: "actions",
    align: "left",
  },
];

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
      columnDefs: stationColumns,
      pagination: {
        rowsPerPage: 15,
        page: 1,
      },
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
    loadStatus() {
      return this.stationsStore.loadStatus;
    },
    isLoaded() {
      return this.stationsStore.loaded;
    },
    loading() {
      return this.stationsStore.loading;
    },
  },
  methods: {
    async reload() {
      // The parameter specifies an artificial delay in milliseconds between
      // load steps, slowing down updates between this.loadStatus updates
      await this.stationsStore.loadFromDb(250);
    },
  },
  async mounted() {
    this.appstateStore.currentSection = this.myName;
    if (!this.appstateStore.manualLoadStations && !this.stationsStore.loaded) {
      await this.stationsStore.loadFromDb(0);
    }
  },
};
</script>

<style lang="scss">
.dbginfo {
  font-style: italic;
  font-size: medium;
  color: #8888aa;
  background-color: #333322;
  margin-left: 5em;
}
.dbgbrighter {
  color: #88aa88;
}
.colStyleId {
  width: 4em;
}
.colStyleName {
  width: 15em;
}
.colStyleAddr {
  width: 20em;
}
.colStyleCity {
  width: 6em;
}
.problem {
  font-style: italic;
  color: #eeaa33;
}
</style>
