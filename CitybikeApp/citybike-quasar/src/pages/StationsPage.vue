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
            could be improved a bit: Move the pagination UI to the top, decrease
            the UI for selecting row count, move the pagination controls to the
            center, improve the distincation between enabled and disabled
            buttons, etc. For now, fixing that is low on my priority list
            though.
          </li>
          <li>
            After navigating to a detail page and then navigating back here, the
            state is lost (search, pagination). Not a great experience. There
            are several ways to fix this, but this is also low priority right
            now.
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
          <li>Show some details on one station when clicking its row</li>
          <li>??? Add a map link ???</li>
        </ul>
      </div>
    </DesignNote>
    <h2 class="q-my-md">{{ myName }}</h2>
    <div class="q-pa-md">
      <q-input
        v-model="searchText"
        outlined
        placeholder="Filter"
        dense
        clearable
        debounce="750"
      >
        <template v-slot:prepend>
          <q-icon name="search" />
        </template>
        <q-tooltip>
          Type text to search in any part of the station data: name (Finnish,
          Swedish, English) or address (Finnish, Swedish).
        </q-tooltip>
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
          v-model:pagination="pagination"
          :loading="loading"
          :rows-per-page-options="[10, 15, 20, 25, 30, 40, 50]"
          table-header-class="tblHeader"
        >
          <template v-slot:top>
            <div class="row fit justify-between">
              <div class="row">
                <div class="q-table__title">Citybike Stations</div>
                <!-- <div class="col self-center">
                  <i
                    >(page {{ props.pagination.page }} of
                    {{ props.pagesNumber }})</i
                  >
                </div> -->
              </div>
              <div class="row">
                <q-btn-toggle
                  v-model="columnSetKey"
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
          <template #body-cell-actions="props">
            <q-td :props="props">
              <div>
                <q-btn
                  label="details"
                  icon-right="forward"
                  @click.stop="navigateRowTarget(props.row)"
                  padding="0 1ex"
                  flat
                  no-caps
                  class="text-primary"
                >
                  <q-tooltip :delay="500">
                    Open station details page
                  </q-tooltip>
                </q-btn>
              </div>
            </q-td>
          </template>
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
    name: "nameSe",
    label: "Name (SE)",
    field: "nameSe",
    align: "left",
    classes: "colStyleName",
    headerClasses: "q-table--col-auto-width",
  },
  {
    name: "nameEn",
    label: "Name (EN/FI)",
    field: "nameEn",
    align: "left",
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
    name: "addrSe",
    label: "Address (SE)",
    field: "addrSe",
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
    name: "citySe",
    label: "City (SE)",
    field: (row) => row.city.CitySe,
    classes: "colStyleCity",
    align: "left",
  },
  {
    // virtual column to put action buttons in
    name: "actions",
    align: "left",
    required: true,
  },
];

const columnSetDefs = {
  FI: ["id", "nameFi", "addrFi", "city", "actions"],
  SE: ["id", "nameSe", "addrSe", "citySe", "actions"],
  EN: ["id", "nameEn", "addrFi", "city", "actions"],
};

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
      columnSets: columnSetDefs,
      columnSetKey: "FI",
      pagination: {
        rowsPerPage: 15,
        page: 1,
      },
      searchText: null,
      filteredStations: null,
      dbg: "",
    };
  },
  components: {
    DesignNote,
  },
  computed: {
    currentColumnSet() {
      return this.columnSets[this.columnSetKey];
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
      // load steps, slowing down updates between this.loadStatus updates
      await this.stationsStore.loadFromDb(250);
    },
    applySearch(txt) {
      if (txt === null || txt === "") {
        this.filteredStations = null;
      } else {
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
        this.filteredStations = l;
        this.pagination.page = 1;
        // console.log("Found stations: " + l.length);
      }
    },
    navigateRowTarget(row) {
      const target = `/stations/${row.id}`;
      console.log(`navigating to: ${target}`);
      this.$router.push(target);
    },
  },
  watch: {
    searchText(newSearch, oldSearch) {
      this.applySearch(newSearch);
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
.tblHeader {
  font-style: italic;
  color: #1ba344;
}
.problem {
  font-style: italic;
  color: #eeaa33;
}
</style>
