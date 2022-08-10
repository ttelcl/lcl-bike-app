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
                  v-model="appstateStore.nameLanguage"
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
          <!--
            Design Note! We solve the "how to show names in different languages?"
            problem here in a different way than on the rides page (for historical
            reasons ...).
            Here we have three separate columns, and we show/hide columns based on
            the language selector.
            To avoid duplication, I introduced a dedicated component for the
            content of the name columns
          -->
          <template #body-cell-nameFi="props">
            <q-td :props="props">
              <StationNameColumn :row="props.row" name-field="nameFi" />
            </q-td>
          </template>
          <template #body-cell-nameSe="props">
            <q-td :props="props">
              <StationNameColumn :row="props.row" name-field="nameSe" />
            </q-td>
          </template>
          <template #body-cell-nameEn="props">
            <q-td :props="props">
              <StationNameColumn :row="props.row" name-field="nameEn" />
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
          <!-- <template #body-cell-actions="props">
            <q-td :props="props">
              <router-link
                :to="`/rides/?dep=${props.row.id}`"
                class="text-green-2"
              >
                TEST
              </router-link>
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
    <div>
      <q-expansion-item
        expand-separator
        label="Hints &amp; tips"
        switch-toggle-side
        v-model="appstateStore.showHintsInStationsList"
      >
        <ul>
          <li>
            Use any of the
            <q-btn
              icon="logout"
              color="primary"
              dense
              size="xs"
              class="q-mx-xs q-px-xs"
            />
            buttons to preselect the station as departure station in the rides
            browser.
          </li>
          <li>
            Use any of the
            <q-btn
              icon="login"
              color="primary"
              dense
              size="xs"
              class="q-mx-xs q-px-xs"
            />
            buttons to preselect the station as return station in the rides
            browser.
          </li>
          <li>Click on column headers to sort.</li>
          <li>
            Click on the link in the Name column to see the details page for the
            station.
          </li>
          <li>
            Type in the <q-icon name="search" size="sm" /> text box to search
            station names and addresses in any of the supported languages.
          </li>
          <li>
            "Rank" is based on total number of incoming and outgoing rides
          </li>
          <li>
            Data only includes rides with a length between 400 m and 8 km.
          </li>
          <li>
            Data only includes rides with a duration between 2 minutes and 4
            hours.
          </li>
          <li>
            Data only includes rides where the duration agrees with the
            difference between the departure and return time (within a 20
            seconds tolerance).
          </li>
        </ul>
      </q-expansion-item>
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
import {
  useStationsStore,
  statsAvgDistance,
  statsAvgDuration,
} from "src/stores/stationsStore";
import { useStationsViewStore } from "../stores/stationsViewStore";
import { useRideCountStore } from "src/stores/rideCountStore";
import { utilities } from "../webapi/utilities";
import DesignNote from "components/DesignNote.vue";
import StationNameColumn from "src/components/StationNameColumn.vue";

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
    name: "rank",
    label: "Rank",
    field: "rideRank",
    classes: "colStyleRideCount",
    align: "right",
    sortable: true,
    required: true,
  },
  {
    name: "depCount",
    label: "Starts",
    field: (row) => row.depStats.count || 0,
    classes: "colStyleRideCount",
    align: "right",
    sortable: true,
    sortOrder: "da",
    required: true,
  },
  {
    name: "retCount",
    label: "Ends",
    field: (row) => row.retStats.count || 0,
    classes: "colStyleRideCount",
    align: "right",
    sortable: true,
    sortOrder: "da",
    required: true,
  },
  {
    name: "rideCount",
    label: "Total",
    field: (row) => row.depStats.count + row.retStats.count || 0,
    classes: "colStyleRideCount",
    align: "right",
    sortable: true,
    sortOrder: "da",
    required: false, // hide by default
  },
  {
    name: "avg_dist_in",
    label: "Avg dist IN",
    field: (row) => statsAvgDistance(row.retStats) / 1000 || 0,
    classes: "colStyleRideCount",
    align: "right",
    sortable: true,
    required: true,
  },
  {
    name: "avg_dist_out",
    label: "Avg dist OUT",
    field: (row) => statsAvgDistance(row.depStats) / 1000 || 0,
    classes: "colStyleRideCount",
    align: "right",
    sortable: true,
    required: true,
  },
  {
    name: "avg_dur_in",
    label: "Avg dur IN",
    field: (row) => utilities.formatTimespan(statsAvgDuration(row.retStats)),
    classes: "colStyleRideCount",
    align: "right",
    sortable: true,
    required: true,
  },
  {
    name: "avg_dur_out",
    label: "Avg dur OUT",
    field: (row) => utilities.formatTimespan(statsAvgDuration(row.depStats)),
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
    "rank",
    "depCount",
    "retCount",
    "actions",
  ],
  SE: [
    "id",
    "nameSe",
    "addrSe",
    "citySe",
    "rank",
    "depCount",
    "retCount",
    "actions",
  ],
  EN: [
    "id",
    "nameEn",
    "addrFi",
    "city",
    "rank",
    "depCount",
    "retCount",
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
      searchText: null, // local version, synced with store via watch
      currentSearch: null,
      filteredStations: null,
    };
  },
  components: {
    DesignNote,
    StationNameColumn,
  },
  computed: {
    currentColumnSet() {
      return this.columnSets[this.appstateStore.nameLanguage];
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
  width: 15rem;
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
