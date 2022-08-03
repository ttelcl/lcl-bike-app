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
                  <td class="propcolVal larger">{{ station.nameFi }}</td>
                  <td class="propcolVal">{{ station.nameSe }}</td>
                  <td class="propcolVal">{{ station.nameEn }}</td>
                </tr>
                <tr>
                  <td class="propcolName larger text-green-14">Address</td>
                  <td class="propcolVal larger">{{ station.addrFi }}</td>
                  <td class="propcolVal">{{ station.addrSe }}</td>
                  <td class="propcolVal"></td>
                </tr>
                <tr>
                  <td class="propcolName larger text-green-14">City</td>
                  <td class="propcolVal larger">{{ station.city.CityFi }}</td>
                  <td class="propcolVal">{{ station.city.CitySe }}</td>
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
                    <span> {{ station.latitude }} </span>
                    <span class="text-green-14"> N, </span>
                    <span> {{ station.longitude }} </span>
                    <span class="text-green-14"> E </span>
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
import DesignNote from "components/DesignNote.vue";

export default {
  name: "StationPage",
  setup() {
    const appstateStore = useAppstateStore();
    const citiesStore = useCitiesStore();
    const stationsStore = useStationsStore();
    return { appstateStore, citiesStore, stationsStore };
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
    this.appstateStore.currentSection = `${this.myName} - ${this.stationNameCompact}`;
  },
};
</script>

<style scoped>
.propcolName {
  width: 7em;
  text-align: right;
  font-style: italic;
}
.propcolVal {
  width: 15em;
  text-align: left;
}
.larger {
  font-size: 120%;
}
</style>
