<template>
  <q-page class="q-pa-md">
    <q-breadcrumbs>
      <q-breadcrumbs-el icon="home" to="/" />
      <q-breadcrumbs-el label="Cities" icon="location_city" />
    </q-breadcrumbs>
    <DesignNote :expand="false">
      <p>This page is a (temporary?) design placeholder.</p>
      <p>
        While "cities" are a concept that exists in my database design, the
        assignment doesn't mention them nor requires any specific functionality
        related to them. I just (mis-)use them here for prototyping UI concepts
        and technologies, before using those technologies on more complex and
        more extensive data. Cities are simple and few, compared to Stations and
        Rides.
      </p>
      <p>
        To further aid functionality checking during implementation, there are
        two versions of the city list: a hardcoded one (initial content of the
        "citiesStore" pinia store) and that same store loaded from the database.
        The hardcoded version uses "<i>((not loaded))</i>" instead of Swedish
        names as a canary.
      </p>
    </DesignNote>
    <h2>{{ myName }}</h2>
    <div class="q-pa-md">
      <q-table
        title="Cities"
        :rows="citiesList"
        :columns="columnDefs"
        row-key="id"
        separator="cell"
        dense
        hide-bottom
      >
        <template #body-cell-actions="props">
          <q-td :props="props">
            <div>
              <q-btn
                label="details"
                icon-right="forward"
                @click.stop="inspectRowTarget(props.row)"
                padding="0 1ex"
                flat
                no-caps
                class="text-primary"
              >
                <q-tooltip :delay="500"> Open details page </q-tooltip>
              </q-btn>
            </div>
          </q-td>
        </template>
      </q-table>
    </div>
    <!-- Unless there is a problem, this section will be invisible -->
    <div class="q-pa-md" v-if="!citiesStore.loaded">
      <p v-if="citiesStore.loaded" class="text-green">Loaded from DB</p>
      <p v-else class="text-orange">Not yet loaded from DB</p>
      <q-btn color="purple" @click="reload">(Re)load</q-btn>
      <p v-if="citiesStore.errorMessage" class="text-red">
        Load error: {{ citiesStore.errorMessage }}
      </p>
    </div>
  </q-page>
</template>

<script>
import { useAppstateStore } from "../stores/appstateStore";
import { useCitiesStore } from "../stores/citiesStore";
import DesignNote from "components/DesignNote.vue";

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
    classes: "q-table--col-auto-width",
    align: "left",
  },
  {
    // virtual column to put action buttons in
    name: "actions",
    label: "Actions",
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
  components: {
    DesignNote,
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
    // rowClicked(event, row, index) {
    //   this.clickedRow = row;
    // },
    // rowDoubleClicked(event, row, index) {
    //   this.clickedRow = row;
    //   if (row) {
    //     const target = `/cities/${row.id}`;
    //     console.log(`navigating to: ${target}`);
    //     this.$router.push(target);
    //   }
    // },
    inspectRowTarget(row) {
      const target = `/cities/${row.id}`;
      console.log(`navigating to: ${target}`);
      this.$router.push(target);
    },
    async reload() {
      await this.citiesStore.loadFromDb();
    },
  },
  async mounted() {
    this.appstateStore.currentSection = this.myName;
    if (!this.citiesStore.loaded) {
      await this.citiesStore.loadFromDb();
    }
  },
};
</script>
