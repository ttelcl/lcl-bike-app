<template>
  <q-page class="q-pa-md">
    <q-breadcrumbs>
      <q-breadcrumbs-el icon="home" to="/" />
      <q-breadcrumbs-el label="Rides" icon="directions_bike" />
    </q-breadcrumbs>
    <h2 class="q-my-md">{{ myName }}</h2>
    <ul>
      <li>All Rides Count: {{ ridesStore.allRidesCount }}</li>
      <!-- <li><q-btn label="fetch" @click="reloadRidesMetadata" /></li> -->
      <li>
        First ride started:
        {{
          date.formatDate(ridesStore.firstRideStart, "YYYY-MM-DD HH:mm:ss (Z)")
        }}
      </li>
      <li>
        Last ride started:
        {{
          date.formatDate(ridesStore.lastRideStart, "YYYY-MM-DD HH:mm:ss (Z)")
        }}
      </li>
    </ul>
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
    </div>
  </q-page>
</template>

<script>
import { date } from "quasar";
import { useAppstateStore } from "../stores/appstateStore";
import { useRidesStore } from "../stores/ridesStore";
import { utilities } from "../webapi/utilities";

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
      rideQuery: this.ridesStore.newRideQuery(15, null, null),
      allRidesCount: 0,
      dateRange: {
        from: "2021/06/01",
        to: "2021/06/30",
      },
    };
  },
  computed: {},
  methods: {
    async getAllRidesCount() {
      const n = await this.ridesStore.getRidesCount(this.rideQuery);
      this.allRideCount = n;
    },
    async reloadRidesMetadata() {
      await this.ridesStore.reload(true);
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
