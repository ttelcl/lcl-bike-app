<template>
  <q-page class="q-pa-md">
    <q-breadcrumbs>
      <q-breadcrumbs-el icon="home" to="/" />
      <q-breadcrumbs-el label="Rides" icon="directions_bike" />
    </q-breadcrumbs>
    <h2 class="q-my-md">{{ myName }}</h2>
    <hr />
    <!--
      Helpful link for solving layout puzzles:
      https://github.com/quasarframework/quasar/blob/dev/ui/src/css/core/flex.sass
    -->
    <!-- The query parameters bar -->
    <div class="row q-gutter-md q-py-sm">
      <div class="col-auto">
        <div class="row q-col-gutter-md">
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
            v-model.number="distMin"
            type="number"
            outlined
            class="numInput"
            label="Min. km"
            debounce="750"
            step="0.001"
            :rules="distanceRules"
            @focus="(input) => input.target.select()"
          />
        </div>
        <div class="row q-col-gutter-md">
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
          <q-input
            v-model.number="distMax"
            type="number"
            outlined
            class="numInput"
            label="Max. km"
            debounce="750"
            step="0.001"
            :rules="distanceRules"
            @focus="(input) => input.target.select()"
          />
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
    <!-- The actual table (or load failure error message) -->
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
                v-model="ridesStore.stationNameLanguage"
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
          Design Note! We solve the "how to show names in different lanaguages?" problem
          here in a different way than on the station page (for historical reasons ...).
          Here we have only one column, and its content adapts to the language selector.
        -->
        <template #body-cell-s_from="props">
          <q-td :props="props">
            <div class="row">
              <div class="col">
                <router-link
                  :to="`/stations/${props.row.depStationId}`"
                  class="text-green-2"
                >
                  {{ ridesStore.getStationName(props.row.depStation) }}
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
                  {{ ridesStore.getStationName(props.row.retStation) }}
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
    <div>
      <q-expansion-item
        expand-separator
        label="Hints &amp; tips"
        switch-toggle-side
        v-model="appstateStore.showHintsInRidesBrowser"
      >
        <ul>
          <li>
            Use any of the
            <q-btn
              icon="search"
              color="primary"
              dense
              size="xs"
              class="q-mx-xs q-px-xs"
            />
            buttons in the To or From columns to filter on the station as
            departure or return station.
          </li>
          <li>
            Use any of the
            <q-btn
              icon="search_off"
              color="red-14"
              dense
              size="xs"
              class="q-mx-xs q-px-xs"
            />
            buttons in the To or From columns to clear the filter for that
            station.
          </li>
          <li>
            Sorry, sorting is not supported in the Rides Browser for performance
            reasons.
          </li>
          <li>
            Use any of the fields in the area above the table to filter the
            visible rides.
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
    <!-- <hr />
    <div class="text-grey-5 text-italic bg-brown-10">
      <h6 class="q-my-sm">Temporary Dev / Debug section</h6>
      <ul>
        <li>All Rides Count: {{ ridesStore.allRidesCount }}</li>
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
    </div> -->
  </q-page>
</template>

<script>
import { date } from "quasar";
import { useAppstateStore } from "../stores/appstateStore";
import { useRidesStore } from "../stores/ridesStore";
import { useStationsStore } from "../stores/stationsStore";
import { utilities } from "../webapi/utilities";

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
    field: (row) => utilities.formatTimespan(row.duration),
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
    const distanceRules = [
      (val) =>
        val === null ||
        (Number.isFinite(val) && val >= 0.0 && val <= 40.0) ||
        "0.000 <= distance <= 40.000",
    ];
    return {
      appstateStore,
      ridesStore,
      stationsStore,
      utilities,
      date,
      stationIdRules,
      distanceRules,
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
    distMin: {
      get() {
        if (!Number.isFinite(this.ridesStore.nextQueryParameters.distMin)) {
          return null;
        } else {
          return this.ridesStore.nextQueryParameters.distMin / 1000;
        }
      },
      set(n) {
        if (!Number.isFinite(n)) {
          const changed = Number.isFinite(
            this.ridesStore.nextQueryParameters.distMin
          );
          this.ridesStore.nextQueryParameters.distMin = null;
          this.parametersChanged = changed;
        } else {
          const n1000 = Math.round(n * 1000);
          const changed = this.ridesStore.nextQueryParameters.distMin !== n1000;
          this.ridesStore.nextQueryParameters.distMin = n1000;
          this.parametersChanged = changed;
        }
      },
    },
    distMax: {
      get() {
        if (!Number.isFinite(this.ridesStore.nextQueryParameters.distMax)) {
          return null;
        } else {
          return this.ridesStore.nextQueryParameters.distMax / 1000;
        }
      },
      set(n) {
        if (!Number.isFinite(n)) {
          const changed = Number.isFinite(
            this.ridesStore.nextQueryParameters.distMax
          );
          this.ridesStore.nextQueryParameters.distMax = null;
          this.parametersChanged = changed;
        } else {
          const n1000 = Math.round(n * 1000);
          const changed = this.ridesStore.nextQueryParameters.distMax !== n1000;
          this.ridesStore.nextQueryParameters.distMax = n1000;
          this.parametersChanged = changed;
        }
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
      this.ridesStore.nextQueryParameters.distMin = null;
      this.ridesStore.nextQueryParameters.distMax = null;
      this.ridesStore.nextQueryParameters.secMin = null;
      this.ridesStore.nextQueryParameters.secMax = null;
      this.parametersChanged = false;
      if (!soft) {
        this.$router.replace({ query: null });
        await this.ridesStore.initTable(
          true,
          15,
          1,
          null,
          null,
          null,
          null,
          null,
          null,
          null,
          null
        );
        this.parametersChanged = false;
      }
    },
    // Apply the query:
    async initTable() {
      this.parametersChanged = false;
      console.log(
        "Page initTable NQP=" +
          JSON.stringify(this.ridesStore.nextQueryParameters)
      );
      await this.ridesStore.initTable(
        true,
        15,
        1,
        this.ridesStore.nextQueryParameters.t0,
        this.ridesStore.nextQueryParameters.t1,
        this.ridesStore.nextQueryParameters.depId,
        this.ridesStore.nextQueryParameters.retId,
        this.ridesStore.nextQueryParameters.distMin,
        this.ridesStore.nextQueryParameters.distMax,
        this.ridesStore.nextQueryParameters.secMin,
        this.ridesStore.nextQueryParameters.secMax
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
    async depSearch(row) {
      if (Number.isFinite(row.depStationId)) {
        if (row.depStationId != this.depStationId) {
          this.depStationId = row.depStationId;
        } else {
          this.depStationId = 0; // clear the departure search
        }
      }
    },
    depMatchesCurrent(row) {
      return (
        Number.isFinite(row.depStationId) &&
        row.depStationId == this.depStationId
      );
    },
    async retSearch(row) {
      if (Number.isFinite(row.retStationId)) {
        if (row.retStationId != this.retStationId) {
          this.retStationId = row.retStationId;
        } else {
          this.retStationId = 0; // clear the departure search
        }
      }
    },
    retMatchesCurrent(row) {
      return (
        Number.isFinite(row.retStationId) &&
        row.retStationId == this.retStationId
      );
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
      const dep = parseInt(this.$route.query.dep);
      if (Number.isFinite(dep)) {
        this.depStationId = dep;
      }
      const ret = parseInt(this.$route.query.ret);
      if (Number.isFinite(ret)) {
        this.retStationId = ret;
      }
      if (/^(\d{4}-\d{2}-\d{2})$/.test(this.$route.query.from)) {
        this.startDate = this.$route.query.from;
      }
      if (/^(\d{4}-\d{2}-\d{2})$/.test(this.$route.query.to)) {
        this.endDate = this.$route.query.to;
      }
      const distmin = parseInt(this.$route.query.distmin);
      if (Number.isFinite(distmin)) {
        this.distMin = distmin / 1000;
      }
      const distmax = parseInt(this.$route.query.distmax);
      if (Number.isFinite(distmax)) {
        this.distMax = distmax / 1000;
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
  width: 16rem;
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
