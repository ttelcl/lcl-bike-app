<template>
  <q-page class="q-pa-md">
    <q-breadcrumbs>
      <q-breadcrumbs-el icon="home" to="/" />
      <q-breadcrumbs-el label="Stations" icon="warehouse" to="/stations" />
      <q-breadcrumbs-el :label="stationName" />
    </q-breadcrumbs>
    <div v-if="station">
      <h2 class="q-my-md">{{ myName }} - {{ stationName }}</h2>
      <div>
        <!-- <h5 class="q-my-sm">Properties:</h5> -->
        <div class="row">
          <div class="offset-md-1">
            <q-markup-table separator="cell" dense bordered>
              <thead>
                <tr>
                  <th></th>
                  <th>FI</th>
                  <th>SE</th>
                  <th>EN</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td class="propcolName larger text-green-14">Name</td>
                  <td class="propcolNameVal larger">{{ station.nameFi }}</td>
                  <td class="propcolNameVal">{{ station.nameSe }}</td>
                  <td class="propcolNameVal">{{ station.nameEn }}</td>
                </tr>
                <tr>
                  <td class="propcolName larger text-green-14">Address</td>
                  <td class="propcolNameVal larger">{{ station.addrFi }}</td>
                  <td class="propcolNameVal">{{ station.addrSe }}</td>
                  <td class="propcolNameVal"></td>
                </tr>
                <tr>
                  <td class="propcolName larger text-green-14">City</td>
                  <td class="propcolNameVal larger">
                    {{ station.city.CityFi }}
                  </td>
                  <td class="propcolNameVal">{{ station.city.CitySe }}</td>
                  <td class="propcolNameVal"></td>
                </tr>
              </tbody>
            </q-markup-table>
          </div>
        </div>
        <div class="row q-pt-sm">
          <div class="offset-md-1">
            <q-markup-table separator="cell" dense bordered>
              <tbody>
                <tr>
                  <td class="propcolName larger text-green-14">Capacity</td>
                  <td class="propcolVal larger">{{ station.capacity }}</td>
                </tr>
                <tr>
                  <td class="propcolName larger text-green-14">Coordinates</td>
                  <td class="propcolVal">
                    <div class="row justify-between">
                      <div class="col-auto">
                        <span> {{ station.latitude }} </span>
                        <span class="text-green-14"> N, </span>
                        <span> {{ station.longitude }} </span>
                        <span class="text-green-14"> E </span>
                      </div>
                      <div class="col-auto">
                        <span class="external-link">
                          (<a
                            :href="stationsStore.googleMapsUrl(station)"
                            target="_blank"
                            rel="noopener noreferrer"
                            title="Opens in new tab"
                          >
                            show in google maps
                            <q-icon name="open_in_new" size="xs" /> </a
                          >)
                        </span>
                      </div>
                    </div>
                  </td>
                </tr>
              </tbody>
            </q-markup-table>
          </div>
        </div>
      </div>
      <div class="q-py-md">
        <!--
          The 'title' attribute is temporary, it will be overridden
          in the top slot cutomization
        -->
        <q-table
          title="Info on rides to or from"
          :rows="linkList"
          :columns="columnDefs"
          row-key="id"
          separator="cell"
          dense
          :bordered="true"
          v-model:pagination="stationFocusStore.pagination"
          :loading="loading"
          :rows-per-page-options="[10, 15, 20, 25, 30, 40, 50]"
          table-header-class="qtblHeader"
        >
          <template v-slot:top>
            <div class="row fit justify-between">
              <div class="row">
                <div class="q-table__title">
                  Info on rides to or from: {{ stationNameEx }}
                </div>
              </div>
              <div class="row">
                <q-btn-toggle
                  v-model="languageInterceptor"
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
          <template #body-cell-name="props">
            <q-td :props="props">
              <div class="row">
                <div class="col">
                  <router-link
                    :to="`/stations/${props.row.id}`"
                    class="text-green-3"
                    v-if="!isLoop(props.row.id)"
                  >
                    {{ appstateStore.getStationName(props.row.station) }}
                  </router-link>
                  <div v-else class="row">
                    <div class="text-blue-3">
                      {{ appstateStore.getStationName(props.row.station) }}
                    </div>
                    <q-icon
                      name="sync_problem"
                      size="xs"
                      color="warning"
                      right
                    />
                  </div>
                </div>
                <div class="col-auto">
                  <!-- <q-btn
                    icon="construction"
                    padding="0 0"
                    dense
                    size="sm"
                    color="warning"
                  /> -->
                </div>
              </div>
            </q-td>
          </template>
        </q-table>
      </div>
    </div>
    <div v-else>
      <h2>Unknown citybike station #{{ stationId }}</h2>
      <div v-if="!isLoaded">
        <q-icon name="warning" size="lg" color="warning" /> {{ loadStatus }}
      </div>
    </div>
    <div class="row q-pt-xl text-warning">
      <h4>
        <q-icon name="construction" />
        This Page is still Under Construction!
        <q-icon name="construction" />
      </h4>
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
import { useRideCountStore } from "src/stores/rideCountStore";
import { useStationFocusStore } from "src/stores/stationFocusStore";
import { utilities } from "../webapi/utilities";
import DesignNote from "components/DesignNote.vue";

function remoteName(row) {
  // TEMPORARY
  return `${row.station.nameFi} (${row.station.city.CityFi})`;
}

const linkColumns = [
  {
    name: "id",
    label: "Id",
    field: "id",
    required: true,
    align: "right",
    classes: "colStyleLinkId",
    sortable: true,
  },
  {
    name: "name",
    label: "Origin or Destination",
    field: (row) => remoteName(row), // TO BE REMOVED
    align: "left",
    classes: "colStyleLinkName",
    sortable: true,
    required: true,
  },
  // {
  //   name: "city",
  //   label: "City",
  //   field: (row) => row.station.city.CityFi,
  //   classes: "colStyleLinkCity",
  //   align: "left",
  //   required: true,
  // },
  {
    name: "rank",
    label: "Rank",
    field: "rank",
    classes: "colStyleLinkRideCount",
    align: "right",
    sortable: true,
    required: true,
  },
  {
    name: "rankin",
    label: "Rank-IN",
    field: "rankIn",
    classes: "colStyleLinkRideCount",
    align: "right",
    sortable: true,
    required: true,
  },
  {
    name: "rankout",
    label: "Rank-OUT",
    field: "rankOut",
    classes: "colStyleLinkRideCount",
    align: "right",
    sortable: true,
    required: true,
  },
  {
    name: "incount",
    label: "Incoming",
    field: (row) => row.incoming.count,
    classes: "colStyleLinkRideCount",
    align: "right",
    sortable: true,
    sortOrder: "da",
  },
  {
    name: "outcount",
    label: "Outgoing",
    field: (row) => row.outgoing.count,
    classes: "colStyleLinkRideCount",
    align: "right",
    sortable: true,
    sortOrder: "da",
  },
  {
    name: "count",
    label: "Total",
    field: (row) => row.count,
    classes: "colStyleLinkRideCount",
    align: "right",
    sortable: true,
    sortOrder: "da",
  },
  {
    name: "avg_dist_in",
    label: "Avg dist IN",
    field: (row) => statsAvgDistance(row.incoming) / 1000 || 0,
    classes: "colStyleLinkRideCount",
    align: "right",
    sortable: true,
    sortOrder: "da",
  },
  {
    name: "avg_dist_out",
    label: "Avg dist OUT",
    field: (row) => statsAvgDistance(row.outgoing) / 1000 || 0,
    classes: "colStyleLinkRideCount",
    align: "right",
    sortable: true,
    sortOrder: "da",
  },
  {
    name: "avg_dur_in",
    label: "Avg dur IN",
    field: (row) => utilities.formatTimespan(statsAvgDuration(row.incoming)),
    classes: "colStyleLinkRideCount",
    align: "right",
    sortable: true,
    sortOrder: "da",
  },
  {
    name: "avg_dur_out",
    label: "Avg dur OUT",
    field: (row) => utilities.formatTimespan(statsAvgDuration(row.outgoing)),
    classes: "colStyleLinkRideCount",
    align: "right",
    sortable: true,
    sortOrder: "da",
  },
  {
    // virtual column to put action buttons in
    name: "actions",
    align: "left",
    required: true,
  },
];

export default {
  name: "StationPage",
  setup() {
    const appstateStore = useAppstateStore();
    const citiesStore = useCitiesStore();
    const stationsStore = useStationsStore();
    const rideCountStore = useRideCountStore();
    const stationFocusStore = useStationFocusStore();
    return {
      appstateStore,
      citiesStore,
      stationsStore,
      rideCountStore,
      stationFocusStore,
    };
  },
  data() {
    return {
      myName: "Citybike Station",
      columnDefs: linkColumns,
    };
  },
  components: {},
  computed: {
    languageInterceptor: {
      // A shim around appstateStore.nameLanguage, which allows
      // adapting the top title bar upon language change
      get() {
        return this.appstateStore.nameLanguage;
      },
      set(v) {
        this.appstateStore.nameLanguage = v;
        this.appstateStore.currentSection = `${this.myName} - ${this.stationNameCompact}`;
      },
    },
    stationsMap() {
      return this.stationsStore.stations;
    },
    stationId() {
      return this.$route.params.id;
    },
    station() {
      return this.stationsStore.stations[this.stationId];
    },
    stationName() {
      return this.station
        ? this.appstateStore.getStationName(this.station)
        : `Unknown station #${this.stationId}`;
    },
    stationNameEx() {
      return this.station
        ? this.appstateStore.getStationName(this.station) +
            " (" +
            this.appstateStore.getStationCity(this.station) +
            ")"
        : `Unknown station #${this.stationId}`;
    },
    stationNameCompact() {
      return this.station ? this.stationName : `#${this.stationId}?`;
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
    linkList() {
      return this.stationFocusStore.linkStatsList;
    },
  },
  methods: {
    isLoop(targetId) {
      const state = this.station && this.station.id == targetId;
      // Explicitly cast to boolean, don't rely on "falsy/truthy"
      return state ? true : false;
    },
  },
  watch: {},
  async mounted() {
    // console.log("Station Page " + JSON.stringify(this.stationId) + ": mounted");
    await this.stationsStore.loadIfNotDoneSoYet();
    this.appstateStore.currentSection = `${this.myName} - ${this.stationNameCompact}`;
    await this.rideCountStore.load(false);
    await this.stationFocusStore.reset(this.stationId);
    this.appstateStore.currentSection = `${this.myName} - ${this.stationNameCompact}`;
  },
  beforeUpdate() {
    // Unusually we need a "beforUpdate" event for this page, sharing part of the
    // functionality of "mounted". Without this, router navigation from one
    // station page to another isn't properly initialized.

    // console.log(
    //   "Station Page " + JSON.stringify(this.stationId) + ": beforeUpdate"
    // );
    this.stationFocusStore.resetCore(this.stationId);
  },
};
</script>

<style>
.propcolName {
  width: 7rem;
  text-align: right;
  font-style: italic;
}
.propcolNameVal {
  width: 10rem;
  text-align: left;
}
.propcolVal {
  width: 30rem;
  text-align: left;
}
.larger {
  font-size: 120%;
}
.colStyleLinkId {
  width: 3rem;
}
.colStyleLinkName {
  width: 15rem;
}
.colStyleLinkAddr {
  width: 16rem;
}
.colStyleLinkCity {
  width: 6rem;
}
.colStyleLinkRideCount {
  width: 4rem;
}
</style>
