<template>
  <q-page class="q-pa-md">
    <q-breadcrumbs>
      <q-breadcrumbs-el icon="home" to="/" />
      <q-breadcrumbs-el label="Cities" icon="location_city" />
    </q-breadcrumbs>
    <div class="design-note-outer">
      <div class="design-note-inner">
        <p>This page is a (temporary?) design placeholder.</p>
        <p>
          While "cities" are a concept that exists in my database design, the
          assignment doesn't mention them nor requires any specific
          functionality related to them. I just (mis-)use them here for
          prototyping UI concepts and technologies, before using those
          technologies on more complex and more extensive data. Cities are
          simple and few, compared to Stations and Rides.
        </p>
        <p>
          To further aid functionality checking during implementation, there are
          two versions of the city list: a hardcoded one (initial content of the
          "citiesStore" pinia store) and that same store loaded from the
          database. The hardcoded version uses "()" instead of Swedish names as
          a canary.
        </p>
      </div>
    </div>
    <h2>{{ myName }}</h2>
    <div>
      <ul>
        <li v-for="city in citiesMap" :key="city.id">
          {{ city.id }} : {{ city.CityFi }} ({{ city.CitySe }})
        </li>
      </ul>
    </div>
    <div class="q-pa-md">
      <q-table
        title="Cities"
        :rows="citiesList"
        :columns="columnDefs"
        row-key="id"
        separator="cell"
        dense
        hide-bottom
        selection="single"
        v-model:selected="selectedCities"
        @row-click="rowClicked"
        @row-dblclick="rowDoubleClicked"
      />
      <!-- @row-click="rowClicked" -->
      <div class="q-mt-md">
        <h5>Selected row:</h5>
        <span v-if="selectedCity"
          >[{{ selectedCity.id }}] {{ selectedCity.CityFi }} ({{
            selectedCity.CitySe
          }})</span
        >
        <span v-else><i>Nothing!</i></span>
      </div>
      <div class="q-mt-md">
        <h5>Clicked row:</h5>
        <span v-if="clickedRow">
          {{ JSON.stringify(clickedRow) }}
        </span>
        <span v-else> No clicks </span>
      </div>
    </div>
  </q-page>
</template>

<script>
import { useAppstateStore } from "../stores/appstateStore";
import { useCitiesStore } from "../stores/citiesStore";

// ref https://quasar.dev/vue-components/table
const cityColumns = [
  {
    name: "id",
    label: "Id",
    field: "id",
    required: true,
    align: "right",
    classes: "q-table--col-auto-width",
    headerClasses: "q-table--col-auto-width",
  },
  {
    name: "cityFi",
    label: "Name (Fi)",
    field: "CityFi",
    align: "left",
    classes: "q-table--col-auto-width",
    headerClasses: "q-table--col-auto-width",
  },
  {
    name: "citySe",
    label: "Name (Se)",
    field: "CitySe",
    align: "left",
  },
];

export default {
  name: "CitiesPage",
  setup() {
    const appstateStore = useAppstateStore();
    const citiesStore = useCitiesStore();
    return { appstateStore, citiesStore };
  },
  data() {
    return {
      myName: "Cities",
      columnDefs: cityColumns,
      selectedCities: [],
      clickedRow: null,
    };
  },
  computed: {
    citiesMap() {
      return this.citiesStore.cities;
    },
    citiesList() {
      return Object.values(this.citiesStore.cities);
    },
    selectedCity() {
      return this.selectedCities.length == 1 ? this.selectedCities[0] : null;
    },
  },
  methods: {
    rowClicked(event, row, index) {
      this.clickedRow = row;
    },
    rowDoubleClicked(event, row, index) {
      this.clickedRow = row;
      if (row) {
        // const target = "/cities/" + row.id;
        const target = `/cities/${row.id}`;
        console.log(`navigating to: ${target}`);
        this.$router.push(target);
      }
    },
  },
  mounted() {
    this.appstateStore.currentSection = this.myName;
  },
};
</script>
