<template>
  <q-page class="q-pa-md">
    <q-breadcrumbs>
      <q-breadcrumbs-el icon="home" to="/" />
      <q-breadcrumbs-el label="Stations" icon="warehouse" to="/stations" />
      <q-breadcrumbs-el :label="stationName" />
    </q-breadcrumbs>
    <div v-if="station">
      <h2 class="q-my-md">{{ myName }} - {{ station.nameFi }}</h2>
      <div>
        <!-- <h5 class="q-my-sm">Properties:</h5> -->
        <div class="row">
          <div class="offset-md-0">
            <q-markup-table separator="cell" dense>
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
                </tr>
              </tbody>
            </q-markup-table>
          </div>
        </div>
        <hr />
        <div class="row">
          <div class="offset-md-0">
            <q-markup-table separator="cell" dense>
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
    </div>
    <div v-else>
      <h2>Unknown citybike station #{{ stationId }}</h2>
      <div v-if="!isLoaded">
        <q-icon name="warning" size="lg" color="warning" /> {{ loadStatus }}
      </div>
    </div>
  </q-page>
</template>

<script>
import { useAppstateStore } from "../stores/appstateStore";
import { useCitiesStore } from "../stores/citiesStore";
import { useStationsStore } from "src/stores/stationsStore";
import { useRideCountStore } from "src/stores/rideCountStore";
import DesignNote from "components/DesignNote.vue";

export default {
  name: "StationPage",
  setup() {
    const appstateStore = useAppstateStore();
    const citiesStore = useCitiesStore();
    const stationsStore = useStationsStore();
    const rideCountStore = useRideCountStore();
    return { appstateStore, citiesStore, stationsStore, rideCountStore };
  },
  data() {
    return {
      myName: "Citybike Station",
      pagination: {
        rowsPerPage: 15,
        page: 1,
      },
    };
  },
  components: {},
  computed: {
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
        ? this.station.nameFi
        : `Unknown station #${this.stationId}`;
    },
    stationNameCompact() {
      return this.station ? this.station.nameFi : `#${this.stationId}?`;
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
    englishDifferent() {
      return this.station.nameFi != this.station.nameEn;
    },
  },
  methods: {},
  watch: {},
  async mounted() {
    if (!this.appstateStore.manualLoadStations && !this.stationsStore.loaded) {
      await this.stationsStore.loadFromDb(0);
    }
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

    this.appstateStore.currentSection = `${this.myName} - ${this.stationNameCompact}`;
  },
};
</script>

<style scoped>
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
</style>
