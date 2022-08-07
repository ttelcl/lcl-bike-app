<template>
  <q-page class="q-pa-md">
    <q-breadcrumbs>
      <q-breadcrumbs-el icon="home" to="/" />
      <q-breadcrumbs-el label="Rides" icon="directions_bike" />
    </q-breadcrumbs>
    <h2 class="q-my-md">{{ myName }}</h2>
    <hr />
    <!-- <h4 class="q-my-md">Query Parameters</h4> -->
    <!--
      Helpful link for solving layout puzzles:
      https://github.com/quasarframework/quasar/blob/dev/ui/src/css/core/flex.sass
    -->
    <!-- The query parameters bar -->
    <div class="row q-gutter-md q-py-sm">
      <div class="col-auto">
        <div class="row q-col-gutter-md">
          <!-- ridesStore.nextQueryParameters.depId -->
          <q-input
            v-model.number="depStationId"
            type="number"
            outlined
            class="numInput"
            label="From Station Id"
            debounce="750"
            :hint="ridesStore.nextDepStationName"
            :rules="stationIdRules"
            ref="depIdField"
            @focus="(input) => input.target.select()"
          />
          <!-- ridesStore.nextQueryParameters.retId -->
          <q-input
            v-model.number="retStationId"
            type="number"
            outlined
            class="numInput"
            label="To Station Id"
            debounce="750"
            :hint="ridesStore.nextRetStationName"
            :rules="stationIdRules"
            ref="retIdField"
            @focus="(input) => input.target.select()"
          />
          <q-input
            v-model="startDate"
            mask="####-##-##"
            outlined
            class="dateInput"
            label="start date"
            readonly
          >
            <template v-slot:append>
              <q-icon name="event" class="cursor-pointer">
                <q-popup-proxy
                  cover
                  transition-show="scale"
                  transition-hide="scale"
                >
                  <q-date
                    mask="YYYY-MM-DD"
                    v-model="startDate"
                    :default-year-month="initialMonth"
                  >
                    <div class="row items-center justify-end">
                      <q-btn v-close-popup label="Close" color="primary" flat />
                    </div>
                  </q-date>
                </q-popup-proxy>
              </q-icon>
            </template>
          </q-input>
          <q-input
            v-model="endDate"
            mask="####-##-##"
            outlined
            class="dateInput"
            label="end date"
            readonly
          >
            <template v-slot:append>
              <q-icon name="event" class="cursor-pointer">
                <q-popup-proxy
                  cover
                  transition-show="scale"
                  transition-hide="scale"
                >
                  <q-date
                    mask="YYYY-MM-DD"
                    v-model="endDate"
                    :default-year-month="finalMonth"
                  >
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
      <div class="column col-auto">
        <div class="row rounded-borders" :class="applyButtonColorClass">
          <q-btn
            label="Apply"
            @click="initTable"
            no-caps
            class="btnWidthHack2"
            :disable="applyDisabled"
          />
          <q-checkbox v-model="ridesStore.autoApplyQuery" dense class="q-pr-xs">
            <q-tooltip> Auto-apply whenever a parameter changes </q-tooltip>
          </q-checkbox>
        </div>
        <div class="row q-pt-sm">
          <q-btn
            label="Reset"
            color="primary"
            @click="resetQuery(false)"
            no-caps
            class="btnWidthHack"
          />
        </div>
      </div>
    </div>
    <hr />
    <div v-if="ridesStore.currentPaginationInitialized">
      <q-table
        title="Rides"
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
        <template v-slot:top>
          <div class="row fit justify-between">
            <div class="row">
              <div class="q-table__title">Rides</div>
            </div>
            <div class="row">
              <q-btn-toggle
                v-model="ridesStore.addressLanguage"
                toggle-color="primary"
                :options="[
                  { label: 'FI', value: 'FI' },
                  { label: 'SE', value: 'SE' },
                ]"
              />
            </div>
          </div>
        </template>
        <template #body-cell-s_from="props">
          <q-td :props="props">
            <div class="row">
              <div class="col">
                <router-link
                  :to="`/stations/${props.row.depStationId}`"
                  class="text-green-2"
                >
                  {{ stationName(props.row.depStation) }}
                </router-link>
              </div>
              <div class="col-auto">
                <q-btn
                  :icon="depMatchesCurrent(props.row) ? 'search_off' : 'search'"
                  @click.stop="depSearch(props.row)"
                  padding="0 0"
                  :color="depMatchesCurrent(props.row) ? 'red-14' : 'primary'"
                  dense
                  size="sm"
                />
              </div>
            </div>
          </q-td>
        </template>
        <template #body-cell-s_to="props">
          <q-td :props="props">
            <div class="row">
              <div class="col">
                <router-link
                  :to="`/stations/${props.row.retStationId}`"
                  class="text-green-2"
                >
                  {{ stationName(props.row.retStation) }}
                </router-link>
              </div>
              <div class="col-auto">
                <q-btn
                  :icon="retMatchesCurrent(props.row) ? 'search_off' : 'search'"
                  @click.stop="retSearch(props.row)"
                  padding="0 0"
                  :color="retMatchesCurrent(props.row) ? 'red-14' : 'primary'"
                  dense
                  size="sm"
                />
              </div>
            </div>
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
  </q-page>
</template>

<script>
import { date } from "quasar";
import { useAppstateStore } from "../stores/appstateStore";
import { useRidesStore } from "../stores/ridesStore";
import { useStationsStore } from "../stores/stationsStore";
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
    // field: (row) => row.depStation.nameFi, // Unused! body cell picks up the right FI/SE name
    align: "left",
    classes: "colWidthStation",
    headerClasses: "colWidthStation",
  },
  {
    name: "s_to",
    label: "To",
    // field: (row) => row.retStation.nameFi, // Unused! body cell picks up the right FI/SE name
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
    const stationsStore = useStationsStore();
    const stationIdRules = [
      (val) => (val >= 0 && val <= 999) || "0 <= ID <= 999",
      (val) => val == 0 || stationsStore.stations[val] || "Unknown station",
    ];
    return {
      appstateStore,
      ridesStore,
      stationsStore,
      utilities,
      date,
      stationIdRules,
    };
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
      depErr: false,
      retErr: false,
      parametersChanged: false,
      queryPending: false,
    };
  },
  computed: {
    startDate: {
      // Originally intended to translate between Date and "YYYY/MM/DD" formats
      // Now both store and state use "YYYY-MM-DD" strings, but have a different
      // representation of "not defined"
      get() {
        const t = this.ridesStore.nextQueryParameters.t0;
        if (t === null) {
          return undefined;
        } else {
          return this.ridesStore.nextQueryParameters.t0;
        }
      },
      set(nt) {
        const changed = this.ridesStore.nextQueryParameters.t0 != nt;
        this.ridesStore.nextQueryParameters.t0 = nt ? nt : null;
        this.parametersChanged = changed;
      },
    },
    endDate: {
      get() {
        const t = this.ridesStore.nextQueryParameters.t1;
        if (t === null) {
          return undefined;
        } else {
          return this.ridesStore.nextQueryParameters.t1;
        }
      },
      set(nt) {
        const changed = this.ridesStore.nextQueryParameters.t1 != nt;
        this.ridesStore.nextQueryParameters.t1 = nt ? nt : null;
        this.parametersChanged = changed;
      },
    },
    retStationId: {
      get() {
        return this.ridesStore.nextQueryParameters.retId;
      },
      set(n) {
        const changed = this.ridesStore.nextQueryParameters.retId != n;
        this.ridesStore.nextQueryParameters.retId = n;
        this.parametersChanged = changed;
      },
    },
    depStationId: {
      get() {
        return this.ridesStore.nextQueryParameters.depId;
      },
      set(n) {
        const changed = this.ridesStore.nextQueryParameters.depId != n;
        this.ridesStore.nextQueryParameters.depId = n;
        this.parametersChanged = changed;
      },
    },
    initialDate() {
      return date.formatDate(this.ridesStore.firstRideStart, "YYYY-MM-DD");
    },
    finalDate() {
      return date.formatDate(this.ridesStore.lastRideStart, "YYYY-MM-DD");
    },
    initialMonth() {
      return date.formatDate(this.ridesStore.firstRideStart, "YYYY/MM");
    },
    finalMonth() {
      return date.formatDate(this.ridesStore.lastRideStart, "YYYY/MM");
    },
    applyDisabled() {
      return this.ridesStore.autoApplyQuery && !this.parametersChanged;
    },
    applyButtonColorClass() {
      if (this.ridesStore.autoApplyQuery) {
        return this.parametersChanged ? "bg-blue" : "bg-grey-9";
      } else {
        return this.parametersChanged ? "bg-warning" : "bg-primary";
      }
    },
  },
  methods: {
    async reloadRidesMetadata() {
      await this.ridesStore.reload(true);
    },
    // "soft=true" only resets the parameters
    // "soft=false" additionally clears the query of the URL and executes the
    // reset query
    async resetQuery(soft = false) {
      this.ridesStore.nextQueryParameters.t0 = null;
      this.ridesStore.nextQueryParameters.t1 = null;
      this.ridesStore.nextQueryParameters.depId = 0;
      this.ridesStore.nextQueryParameters.retId = 0;
      this.parametersChanged = false;
      if (!soft) {
        this.$router.replace({ query: null });
        await this.ridesStore.initTable(true, 15, 1, null, null, null, null);
        this.parametersChanged = false;
      }
    },
    // Apply the query:
    async initTable() {
      this.parametersChanged = false;
      await this.ridesStore.initTable(
        true,
        15,
        1,
        this.ridesStore.nextQueryParameters.t0,
        this.ridesStore.nextQueryParameters.t1,
        this.ridesStore.nextQueryParameters.depId,
        this.ridesStore.nextQueryParameters.retId
      );
      this.queryPending = false;
    },
    async updateTablePage(props) {
      await this.ridesStore.updateTablePage(props);
    },
    startQuery() {
      if (!this.queryPending) {
        this.initTable().then(() => this.finishQuery());
      } else {
        console.log(
          "Not starting new query while previous one is still running"
        );
      }
    },
    finishQuery() {
      this.queryPending = false;
    },
    stationName(station) {
      const lang = this.ridesStore.addressLanguage;
      return lang == "SE" ? station.nameSe : station.nameFi;
    },
    async depSearch(row) {
      if (!isNaN(row.depStationId)) {
        if (row.depStationId != this.depStationId) {
          this.depStationId = row.depStationId;
        } else {
          this.depStationId = 0; // clear the departure search
        }
      }
    },
    depMatchesCurrent(row) {
      return !isNaN(row.depStationId) && row.depStationId == this.depStationId;
    },
    async retSearch(row) {
      if (!isNaN(row.retStationId)) {
        if (row.retStationId != this.retStationId) {
          this.retStationId = row.retStationId;
        } else {
          this.retStationId = 0; // clear the departure search
        }
      }
    },
    retMatchesCurrent(row) {
      return !isNaN(row.retStationId) && row.retStationId == this.retStationId;
    },
  },
  watch: {
    // tmpDepId(n, o) {
    //   if (n != this.ridesStore.nextQueryParameters.depId) {
    //     this.ridesStore.nextQueryParameters.depId = n;
    //     this.parametersChanged = true;
    //   }
    // },
    // tmpRetId(n, o) {
    //   if (n != this.ridesStore.nextQueryParameters.retId) {
    //     this.ridesStore.nextQueryParameters.retId = n;
    //     this.parametersChanged = true;
    //   }
    // },
    parametersChanged(n, o) {
      if (n && this.ridesStore.autoApplyQuery) {
        this.startQuery();
      }
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
    if (!this.ridesStore.currentPaginationInitialized) {
      await this.resetQuery(true);
    }
    const oldAutoApply = this.ridesStore.autoApplyQuery;
    try {
      if (!isNaN(this.$route.query.dep)) {
        this.depStationId = this.$route.query.dep;
      }
      if (!isNaN(this.$route.query.ret)) {
        this.retStationId = this.$route.query.ret;
      }
      if (/^(\d{4}-\d{2}-\d{2})$/.test(this.$route.query.from)) {
        this.startDate = this.$route.query.from;
      }
      if (/^(\d{4}-\d{2}-\d{2})$/.test(this.$route.query.to)) {
        this.endDate = this.$route.query.to;
      }
    } finally {
      this.ridesStore.autoApplyQuery = oldAutoApply;
    }
    await this.initTable();
  },
};
</script>

<style lang="scss">
.colWidthStation {
  width: 13rem;
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

.dateInput {
  width: 10rem;
}

.btnWidthHack {
  min-width: 7rem;
}

.btnWidthHack2 {
  min-width: 5rem;
}

// Remove adornments from number inputs:
.numInput {
  width: 10rem;
}
.numInput input[type="number"]::-webkit-outer-spin-button,
.numInput input[type="number"]::-webkit-inner-spin-button {
  // Chrome / Edge
  -webkit-appearance: none;
  margin: 0;
}
.numInput input[type="number"] {
  // Firefox
  -moz-appearance: textfield;
}
</style>
