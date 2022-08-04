<template>
  <q-page class="q-pa-md">
    <q-breadcrumbs>
      <q-breadcrumbs-el icon="home" to="/" />
      <q-breadcrumbs-el label="Rides" icon="directions_bike" />
    </q-breadcrumbs>
    <h2 class="q-my-md">{{ myName }}</h2>
    <div>
      <div class="row q-gutter-md">
        <div class="row">
          <q-btn
            label="Reset table (server-backed pagination)"
            color="purple"
            @click="initTable"
            no-caps
          />
        </div>
      </div>
    </div>
    <hr />
    <div v-if="ridesStore.currentPaginationInitialized">
      <q-table
        title="Rides (server-side pagination)"
        :rows="ridesStore.currentPageRows"
        :columns="ridesColumns"
        row-key="id"
        separator="cell"
        dense
        :bordered="true"
        v-model:pagination="ridesStore.currentPagination"
        :rows-per-page-options="[10, 15, 20, 25, 30, 40, 50]"
        table-header-class="qtblHeader"
        :loading="ridesStore.loading"
        @request="updateTablePage"
      >
        <template #body-cell-s_from="props">
          <q-td :props="props">
            <router-link
              :to="`/stations/${props.row.depStationId}`"
              class="text-green-2"
              >{{ props.row.depStation.nameFi }}</router-link
            >
          </q-td>
        </template>
        <template #body-cell-s_to="props">
          <q-td :props="props">
            <router-link
              :to="`/stations/${props.row.retStationId}`"
              class="text-green-2"
              >{{ props.row.retStation.nameFi }}</router-link
            >
          </q-td>
        </template>
      </q-table>
    </div>
    <div v-else>
      <div>Not yet initialized.</div>
    </div>
    <hr />
    <div class="text-grey-5 text-italic bg-brown-10">
      <h6 class="q-my-sm">Temporary Dev / Debug section</h6>
      <ul>
        <li>All Rides Count: {{ ridesStore.allRidesCount }}</li>
        <!-- <li><q-btn label="fetch" @click="reloadRidesMetadata" /></li> -->
        <li>
          First ride started:
          {{
            date.formatDate(
              ridesStore.firstRideStart,
              "YYYY-MM-DD HH:mm:ss (Z)"
            )
          }}
        </li>
        <li>
          Last ride started:
          {{
            date.formatDate(ridesStore.lastRideStart, "YYYY-MM-DD HH:mm:ss (Z)")
          }}
        </li>
      </ul>
    </div>
    <!--  -- experiments in date range UI
    <div class="row">
      <div class="col q-pa-sm">
        From:
        <q-input
          filled
          v-model="dateRange.from"
          mask="date"
          dense
          :rules="['date']"
        >
          <template v-slot:append>
            <q-icon name="event" class="cursor-pointer">
              <q-popup-proxy
                cover
                transition-show="scale"
                transition-hide="scale"
              >
                <q-date v-model="dateRange.from">
                  <div class="row items-center justify-end">
                    <q-btn v-close-popup label="Close" color="primary" flat />
                  </div>
                </q-date>
              </q-popup-proxy>
            </q-icon>
          </template>
        </q-input>
      </div>
      <div class="col q-pa-sm">
        To:
        <q-input
          filled
          v-model="dateRange.to"
          mask="date"
          dense
          :rules="['date']"
        >
          <template v-slot:append>
            <q-icon name="event" class="cursor-pointer">
              <q-popup-proxy
                cover
                transition-show="scale"
                transition-hide="scale"
              >
                <q-date v-model="dateRange.to">
                  <div class="row items-center justify-end">
                    <q-btn v-close-popup label="Close" color="primary" flat />
                  </div>
                </q-date>
              </q-popup-proxy>
            </q-icon>
          </template>
        </q-input>
      </div>
    </div>
    <div class="row">
      <q-date v-model="dateRange" range landscape> </q-date>
    </div> -->
  </q-page>
</template>

<script>
import { date } from "quasar";
import { useAppstateStore } from "../stores/appstateStore";
import { useRidesStore } from "../stores/ridesStore";
import { utilities } from "../webapi/utilities";

function formatTimespan(totalSeconds) {
  var rounded = Math.floor(totalSeconds);
  var seconds = rounded % 60;
  var totalMinutes = (rounded - seconds) / 60;
  var minutes = totalMinutes % 60;
  var hours = (totalMinutes - minutes) / 60;
  return (
    hours.toString() +
    ":" +
    minutes.toString().padStart(2, "0") +
    ":" +
    seconds.toString().padStart(2, "0")
  );
}

const ridesColumns = [
  // {
  //   // There must be an ID key in a q-table, but it doesn't need to show as column!
  //   name: "id",
  //   label: "Id",
  //   field: "id",
  //   required: true,
  //   align: "left",
  //   classes: "q-table--col-auto-width",
  //   headerClasses: "q-table--col-auto-width",
  // },
  {
    name: "s_from",
    label: "From",
    field: (row) => row.depStation.nameFi,
    align: "left",
    classes: "colWidthStation",
    headerClasses: "colWidthStation",
  },
  {
    name: "s_to",
    label: "To",
    field: (row) => row.retStation.nameFi,
    align: "left",
    classes: "colWidthStation",
    headerClasses: "colWidthStation",
  },
  {
    name: "t_day",
    label: "Day",
    field: (row) => date.formatDate(row.depTime, "YYYY-MM-DD"),
    align: "right",
    classes: "colWidthDay",
    headerClasses: "colWidthDay",
  },
  {
    name: "t_start",
    label: "Start",
    field: (row) => date.formatDate(row.depTime, "HH:mm:ss"),
    align: "right",
    classes: "colWidthTime",
    headerClasses: "colWidthTime",
  },
  {
    name: "t_return",
    label: "Return",
    field: (row) => date.formatDate(row.retTime, "HH:mm:ss"),
    align: "right",
    classes: "colWidthTime",
    headerClasses: "colWidthTime",
  },
  {
    name: "duration",
    label: "Duration",
    field: (row) => formatTimespan(row.duration),
    align: "right",
    classes: "colWidthDuration",
    headerClasses: "colWidthDuration",
  },
  {
    name: "distance",
    label: "Distance (km)",
    field: (row) => (row.distance / 1000).toFixed(3),
    align: "right",
    classes: "colWidthDistance",
    headerClasses: "colWidthDistance",
  },
  {
    // virtual column to put action buttons in
    name: "actions",
    align: "left",
  },
];

export default {
  name: "RidesPage",
  setup() {
    const appstateStore = useAppstateStore();
    const ridesStore = useRidesStore();
    return { appstateStore, ridesStore, utilities, date };
  },
  data() {
    return {
      myName: "Rides Browser",
      ridesColumns: ridesColumns,
      rideQuery: this.ridesStore.newRideQuery(15, null, null),
      dateRange: {
        from: "2021/06/01",
        to: "2021/06/30",
      },
    };
  },
  computed: {},
  methods: {
    async reloadRidesMetadata() {
      await this.ridesStore.reload(true);
    },
    async initTable() {
      await this.ridesStore.initTable(true, 15, 1, null, null);
    },
    async updateTablePage(props) {
      await this.ridesStore.updateTablePage(props);
    },
  },
  async mounted() {
    this.appstateStore.currentSection = this.myName;
    if (!this.ridesStore.loaded) {
      await this.ridesStore.reload(true);
      if (this.ridesStore.loaded) {
        this.dateRange.from = date.formatDate(
          this.ridesStore.firstRideStart,
          "YYYY/MM/DD"
        );
        this.dateRange.to = date.formatDate(
          this.ridesStore.lastRideStart,
          "YYYY/MM/DD"
        );
      }
    }
  },
};
</script>

<style lang="scss">
.colWidthStation {
  width: 12rem;
}
.colWidthDay {
  width: 6rem;
}
.colWidthTime {
  width: 5rem;
}
.colWidthDuration {
  width: 4rem;
}
.colWidthDistance {
  width: 6rem;
}
</style>
