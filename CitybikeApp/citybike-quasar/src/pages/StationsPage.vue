<template>
  <q-page class="q-pa-md">
    <q-breadcrumbs>
      <q-breadcrumbs-el icon="home" to="/" />
      <q-breadcrumbs-el label="Stations" icon="warehouse" />
    </q-breadcrumbs>
    <DesignNote>
      <div>
        <p class="q-my-none">Known Issues:</p>
        <ul class="q-my-none">
          <li>
            I am not really happy with the out-of-the box table pagination UI
            provided by Quasar. I mean - functionally it is great, but the UI
            could be improved a bit: Move the pagination UI to the top, simplify
            the UI for selecting the row count, move the pagination controls to
            the center, improve the distinction between enabled and disabled
            buttons, etc. For now, fixing that is low on my priority list
            though.
          </li>
        </ul>
        <p class="q-my-none">To Do:</p>
        <ul class="q-my-none">
          <li>
            <s>
              Add a link to a per-station page to act as entry point for a lot
              more details
            </s>
          </li>
          <li>
            <s>
              Make the state of pagination, search and language hint persist
              through navigation.
            </s>
          </li>
          <li>Add a better pagination control.</li>
        </ul>
      </div>
    </DesignNote>
    <h2 class="q-my-md">{{ myName }}</h2>
    <div class="q-pa-md">
      <q-input
        v-model="searchText"
        outlined
        placeholder="Search for a station name or address (in any of the supported languages)"
        dense
        clearable
        debounce="750"
      >
        <!--
          I would like to use "viewStore.searchText" as model, but that would break
          the watch. So instead I store the searchText in two spots and try to keep
          them in sync...
        -->
        <template v-slot:prepend>
          <q-icon name="search" />
        </template>
      </q-input>
      <div>
        <q-table
          title="Citybike Stations"
          :rows="stationsListFiltered"
          :columns="columnDefs"
          :visible-columns="currentColumnSet"
          row-key="id"
          separator="cell"
          dense
          :bordered="true"
          v-model:pagination="viewStore.pagination"
          :loading="loading"
          :rows-per-page-options="[10, 15, 20, 25, 30, 40, 50]"
          table-header-class="qtblHeader"
        >
          <template v-slot:top>
            <div class="row fit justify-between">
              <div class="row">
                <div class="q-table__title">Citybike Stations</div>
              </div>
              <div class="row">
                <q-btn-toggle
                  v-model="viewStore.columnSetKey"
                  toggle-color="primary"
                  :options="[
                    { label: 'FI', value: 'FI' },
                    { label: 'SE', value: 'SE' },
                    { label: 'EN', value: 'EN' },
                  ]"
                />
              </div>
            </div>
          </template>
          <!-- <template #body-cell-actions="props">
            <q-td :props="props">
              <div class="q-gutter-xs">
                <q-btn
                  icon-right="info_outline"
                  @click.stop="navigateRowTarget(props.row)"
                  padding="0 1ex"
                  color="grey-9"
                  text-color="primary"
                >
                  <q-tooltip :delay="500">
                    Open station details page
                  </q-tooltip>
                </q-btn>
              </div>
            </q-td>
          </template> -->
          <template #body-cell-nameFi="props">
            <q-td :props="props">
              <router-link
                :to="`/stations/${props.row.id}`"
                class="text-green-2"
                >{{ props.row.nameFi }}</router-link
              >
            </q-td>
          </template>
          <template #body-cell-nameSe="props">
            <q-td :props="props">
              <router-link
                :to="`/stations/${props.row.id}`"
                class="text-green-2"
                >{{ props.row.nameSe }}</router-link
              >
            </q-td>
          </template>
          <template #body-cell-nameEn="props">
            <q-td :props="props">
              <router-link
                :to="`/stations/${props.row.id}`"
                class="text-green-2"
                >{{ props.row.nameEn }}</router-link
              >
            </q-td>
          </template>
          <template #body-cell-addrFi="props">
            <q-td :props="props">
              <span> {{ props.row.addrFi }} </span>
              <span class="external-link">
                <a
                  :href="stationsStore.googleMapsUrl(props.row)"
                  target="_blank"
                  rel="noopener noreferrer"
                  title="Show in Google Maps in new tab"
                >
                  <!-- <q-icon right name="language" size="xs" /> -->
                  <q-icon right name="open_in_new" size="xs" />
                </a>
              </span>
            </q-td>
          </template>
          <template #body-cell-addrSe="props">
            <q-td :props="props">
              <span> {{ props.row.addrSe }} </span>
              <span class="external-link">
                <a
                  :href="stationsStore.googleMapsUrl(props.row)"
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  <q-icon right name="open_in_new" />
                </a>
              </span>
            </q-td>
          </template>
          <!-- map link functionality into address columns -->
          <!-- <template #body-cell-actions="props">
            <q-td :props="props">
              <span class="external-link">
                <a
                  :href="googleMapsUrl(props.row)"
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  on map
                  <q-icon right name="open_in_new" />
                </a>
              </span>
            </q-td>
          </template> -->
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
import { useStationsViewStore } from "../stores/stationsViewStore";
import { useRideCountStore } from "src/stores/rideCountStore";
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
    // headerClasses: "q-table--col-auto-width",
    sortable: true,
  },
  {
    name: "nameFi",
    label: "Name (FI)",
    field: "nameFi",
    align: "left",
    classes: "colStyleName",
    sortable: true,
  },
  {
    name: "nameSe",
    label: "Name (SE)",
    field: "nameSe",
    align: "left",
    classes: "colStyleName",
    sortable: true,
  },
  {
    name: "nameEn",
    label: "Name (EN/FI)",
    field: "nameEn",
    align: "left",
    classes: "colStyleName",
    sortable: true,
  },
  {
    name: "addrFi",
    label: "Address (FI)",
    field: "addrFi",
    classes: "colStyleAddr",
    align: "left",
    sortable: true,
  },
  {
    name: "addrSe",
    label: "Address (SE)",
    field: "addrSe",
    classes: "colStyleAddr",
    align: "left",
    sortable: true,
  },
  {
    name: "city",
    label: "City",
    field: (row) => row.city.CityFi,
    classes: "colStyleCity",
    align: "left",
  },
  {
    name: "citySe",
    label: "City (SE)",
    field: (row) => row.city.CitySe,
    classes: "colStyleCity",
    align: "left",
  },
  {
    name: "depCount",
    label: "Starts",
    field: "depCount",
    classes: "colStyleRideCount",
    align: "right",
    sortable: true,
    required: true,
  },
  {
    name: "retCount",
    label: "Ends",
    field: "retCount",
    classes: "colStyleRideCount",
    align: "right",
    sortable: true,
    required: true,
  },
  {
    name: "rideCount",
    label: "Total",
    field: (row) => row.depCount + row.retCount,
    classes: "colStyleRideCount",
    align: "right",
    sortable: true,
    required: false, // hide by default
  },
  {
    name: "rank",
    label: "Rank",
    field: "rideRank",
    classes: "colStyleRideCount",
    align: "right",
    sortable: true,
    required: true,
  },
  {
    // virtual column to put action buttons in
    name: "actions",
    align: "left",
    required: true,
  },
];

const columnSetDefs = {
  FI: [
    "id",
    "nameFi",
    "addrFi",
    "city",
    "depCount",
    "retCount",
    "rank",
    "actions",
  ],
  SE: [
    "id",
    "nameSe",
    "addrSe",
    "citySe",
    "depCount",
    "retCount",
    "rank",
    "actions",
  ],
  EN: [
    "id",
    "nameEn",
    "addrFi",
    "city",
    "depCount",
    "retCount",
    "rank",
    "actions",
  ],
};

export default {
  name: "StationsPage",
  setup() {
    const appstateStore = useAppstateStore();
    const citiesStore = useCitiesStore();
    const stationsStore = useStationsStore();
    const viewStore = useStationsViewStore();
    const rideCountStore = useRideCountStore();
    return {
      appstateStore,
      citiesStore,
      stationsStore,
      viewStore,
      rideCountStore,
    };
  },
  data() {
    return {
      myName: "Citybike Station Index",
      columnDefs: stationColumns,
      columnSets: columnSetDefs,
      searchText: null,
      currentSearch: null,
      filteredStations: null,
    };
  },
  components: {
    DesignNote,
  },
  computed: {
    currentColumnSet() {
      return this.columnSets[this.viewStore.columnSetKey];
    },
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
    stationsListFiltered() {
      return this.filteredStations === null
        ? this.stationsList
        : this.filteredStations;
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
      // load steps, slowing down updates between this.loadStatus updates.
      // This is just for demo/debug purposes.
      await this.load(250);
    },
    async load(debugDelay = 0) {
      await this.stationsStore.loadFromDb(debugDelay);
      await this.rideCountStore.load();
    },
    applySearch(txt) {
      const oldCurrentSearch = this.currentSearch;
      const oldLastSearch = this.viewStore.lastSearch;
      if (txt === null || txt === "") {
        this.filteredStations = null;
        this.currentSearch = null;
      } else {
        if (txt != this.currentSearch) {
          // change or restore
          var l = [];
          for (const station of this.stationsList) {
            txt = txt.toLowerCase();
            if (
              station.nameFi.toLowerCase().includes(txt) ||
              station.nameSe.toLowerCase().includes(txt) ||
              station.nameEn.toLowerCase().includes(txt) ||
              station.addrFi.toLowerCase().includes(txt) ||
              station.addrSe.toLowerCase().includes(txt)
            ) {
              l.push(station);
            }
          }
          this.currentSearch = txt;
          this.filteredStations = l;
          // console.log("Found stations: " + l.length);
        }
      }
      if (oldLastSearch != txt) {
        this.viewStore.pagination.page = 1;
      }
      this.viewStore.lastSearch = txt;
    },
    navigateRowTarget(row) {
      const target = `/stations/${row.id}`;
      console.log(`navigating to: ${target}`);
      this.$router.push(target);
    },
  },
  watch: {
    searchText(newSearch, oldSearch) {
      this.viewStore.searchText = newSearch;
      this.applySearch(newSearch);
    },
  },
  async mounted() {
    this.appstateStore.currentSection = this.myName;
    this.searchText = this.viewStore.searchText;
    if (!this.appstateStore.manualLoadStations) {
      if (!this.stationsStore.loaded) {
        await this.load(0);
      } else {
        // This branch is relevant when something else loaded the
        // stationsStore already, but nothing loaded the ride counts
        // yet. Example flow where that happens: Home -> Rides -> Stations.
        await this.rideCountStore.load();
      }
    }
    if (this.$route.query.search) {
      const search = this.$route.query.search;
      this.viewStore.searchText = search;
      this.searchText = search;
    }
    this.applySearch(this.viewStore.searchText); // restore search state
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
  width: 3rem;
}
.colStyleName {
  width: 12rem;
}
.colStyleAddr {
  width: 16rem;
}
.colStyleCity {
  width: 6rem;
}
.colStyleRideCount {
  width: 4rem;
}
.problem {
  font-style: italic;
  color: #eeaa33;
}
</style>
