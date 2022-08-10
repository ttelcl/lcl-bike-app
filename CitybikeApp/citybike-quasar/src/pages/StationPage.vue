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
                  Info on rides to or from:
                  <span class="text-blue-3"> {{ stationNameEx }} </span>
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
                  <div class="row items-baseline" v-if="!isLoop(props.row.id)">
                    <router-link
                      :to="`/stations/${props.row.id}`"
                      class="text-green-3"
                    >
                      {{ appstateStore.getStationName(props.row.station) }}
                    </router-link>
                    <!-- <span class="smaller">
                      &nbsp; ({{
                        appstateStore.getStationCity(props.row.station)
                      }})
                    </span> -->
                  </div>
                  <div v-else class="row">
                    <div class="row items-baseline">
                      <span class="text-blue-3">
                        {{ appstateStore.getStationName(props.row.station) }}
                      </span>
                      <!-- <span class="smaller">
                        &nbsp; ({{
                          appstateStore.getStationCity(props.row.station)
                        }})
                      </span> -->
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
          <template #body-cell-incount="props">
            <q-td :props="props">
              <div class="row">
                <div class="col">
                  {{ props.row.incoming.count }}
                </div>
                <div class="col-auto">
                  <q-btn
                    icon="directions_bike"
                    size="xs"
                    color="primary"
                    dense
                    class="q-px-xs q-ml-xs"
                    :to="`/rides?dep=${props.row.id}&ret=${stationId}`"
                  >
                    <q-tooltip :delay="500" class="text-body2">
                      Show rides from
                      <span class="text-green-3">
                        {{ appstateStore.getStationName(props.row.station) }}
                      </span>
                      to
                      <span class="text-blue-3"> {{ stationName }} </span>
                    </q-tooltip>
                  </q-btn>
                </div>
              </div>
            </q-td>
          </template>
          <template #body-cell-outcount="props">
            <q-td :props="props">
              <div class="row">
                <div class="col">
                  {{ props.row.outgoing.count }}
                </div>
                <div class="col-auto">
                  <q-btn
                    icon="directions_bike"
                    size="xs"
                    color="primary"
                    dense
                    class="q-px-xs q-ml-xs"
                    :to="`/rides?dep=${stationId}&ret=${props.row.id}`"
                  >
                    <q-tooltip :delay="500" class="text-body2">
                      Show rides from
                      <span class="text-blue-3"> {{ stationName }} </span>
                      to
                      <span class="text-green-3">
                        {{ appstateStore.getStationName(props.row.station) }}
                      </span>
                    </q-tooltip>
                  </q-btn>
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
    <div>
      <q-expansion-item
        expand-separator
        label="Hints &amp; tips"
        switch-toggle-side
        v-model="appstateStore.showHintsInStationPage"
      >
        <ul>
          <li>
            Use the
            <q-btn
              icon="directions_bike"
              color="primary"
              dense
              size="xs"
              class="q-mx-xs q-px-xs"
            />
            button in the "Incoming" column to see rides from the row's station
            to <span class="text-blue-3"> {{ stationName }} </span>.
          </li>
          <li>
            Use the
            <q-btn
              icon="directions_bike"
              color="primary"
              dense
              size="xs"
              class="q-mx-xs q-px-xs"
            />
            button in the "Outgoing" column to see rides from
            <span class="text-blue-3"> {{ stationName }} </span> to the row's
            station.
          </li>
          <li>Click on column headers to sort.</li>
          <li>
            Click on the link in the "Origin or Destination" column to visit the
            details page for the row's station.
          </li>
          <li>
            The <q-icon name="sync_problem" size="xs" color="warning" /> icon
            indicates the row for roundtrip rides both starting and ending at
            <span class="text-blue-3"> {{ stationName }} </span>.
          </li>
          <li>
            "Rank" is based on total number of incoming and outgoing rides.
          </li>
          <li>
            There are also separate columns for rank based on incoming or
            outgoing rides only. Sort on those columns to see the most popular
            origin or destination stations for
            <span class="text-blue-3"> {{ stationName }} </span>.
          </li>
          <li>
            Or, equivalently, sort on the Incoming or Outgoing columns (they
            sort biggest first, showing the most popular stations on top).
          </li>
          <li>
            Only stations with any rides to or from
            <span class="text-blue-3"> {{ stationName }} </span> are shown in
            the table.
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

    <!-- <div class="row q-pt-xl text-warning">
      <h4>
        <q-icon name="construction" />
        This Page is still Under Construction!
        <q-icon name="construction" />
      </h4>
    </div> -->
  </q-page>
</template>

<script>
import { useAppstateStore, stationName } from "../stores/appstateStore";
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
    field: (row) => stationName(row.station), // value is used for sorting
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
    name: "count",
    label: "Total",
    field: (row) => row.count,
    classes: "colStyleLinkRideCount",
    align: "right",
    sortable: true,
    sortOrder: "da",
  },
  {
    name: "incount",
    label: "Incoming",
    field: (row) => row.incoming.count,
    classes: "colStyleLinkCount2",
    align: "right",
    sortable: true,
    sortOrder: "da",
  },
  {
    name: "outcount",
    label: "Outgoing",
    field: (row) => row.outgoing.count,
    classes: "colStyleLinkCount2",
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
    // station page to another isn't properly initialized (since the "mounted" event
    // doesn't fire for that case)

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
.smaller {
  font-size: 80%;
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
.colStyleLinkCount2 {
  width: 5rem;
}
</style>
