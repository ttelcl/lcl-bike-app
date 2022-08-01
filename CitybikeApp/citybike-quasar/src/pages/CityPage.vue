<template>
  <q-page class="q-pa-md">
    <q-breadcrumbs>
      <q-breadcrumbs-el icon="home" to="/" />
      <q-breadcrumbs-el label="Cities" icon="location_city" to="/cities" />
      <q-breadcrumbs-el :label="city.CityFi" />
    </q-breadcrumbs>
    <div v-if="city">
      <h2 class="q-my-md">{{ myName }} - {{ city.CityFi }}</h2>
      <div>
        <ul>
          <li>ID = {{ city.id }}</li>
          <li>Name (Finnish) = {{ city.CityFi }}</li>
          <li>Name (Swedish) = {{ city.CitySe }}</li>
        </ul>
      </div>
    </div>
    <div v-else>
      <h2>Unknown city #{{ cityId }}</h2>
    </div>
  </q-page>
</template>

<script>
import { useAppstateStore } from "../stores/appstateStore";
import { useCitiesStore } from "../stores/citiesStore";

export default {
  name: "CityPage",
  setup() {
    const appstateStore = useAppstateStore();
    const citiesStore = useCitiesStore();
    return { appstateStore, citiesStore };
  },
  data() {
    return {
      myName: "City",
    };
  },
  computed: {
    cityId() {
      return this.$route.params.id;
    },
    city() {
      return this.citiesStore.cities[this.$route.params.id];
    },
  },
  methods: {},
  mounted() {
    this.appstateStore.currentSection = this.myName;
  },
};
</script>
