<template>
  <q-page class="q-pa-md">
    <q-breadcrumbs>
      <q-breadcrumbs-el label="Home" icon="home" />
    </q-breadcrumbs>
    <h2 class="q-my-md">{{ myName }}</h2>
    <div class="row">
      <div class="col-md-8 col-lg-6">
        <h4 class="q-my-md">About</h4>
        <div class="q-pl-xl">
          <p>
            This site provides access to information and statistics on the usage
            of the Helsinki & Espoo citybike system, showing data from May-July
            2021.
          </p>
          <p>
            This site was developed as an educational assignment for
            <span class="text-italic text-orange-4">
              [to be completed later...]
            </span>
          </p>
        </div>
      </div>
    </div>
    <div class="row">
      <div class="col">
        <div class="row q-gutter-md q-py-sm">
          <q-btn
            label="Citybike Stations"
            color="primary"
            icon-right="warehouse"
            no-caps
            class="col-3"
            to="/stations"
          />
          <div class="col-6">
            Browse the citybike stations. Search for stations by name or
            address. Explore popularity of stations.
          </div>
        </div>
        <div class="row q-gutter-md q-py-sm">
          <q-btn
            label="Rides"
            color="primary"
            icon-right="directions_bike"
            no-caps
            class="col-3"
            to="/rides"
          />
          <div class="col-6">
            Browse rides. Look for rides starting or ending at a specific
            station. Look for rides in a particular date range. Look for rides
            in a particular range of distances. Look for rides in a particular
            range of duration.
          </div>
        </div>
      </div>
    </div>
    <div class="row q-pt-xl text-warning">
      <h4>
        <q-icon name="construction" />
        This Site is still Under Construction!
        <q-icon name="construction" />
      </h4>
    </div>
    <div class="q-mt-xl">
      <hr />
      <DesignNote :expand="false" title="Development Settings" :alt="true">
        <ul class="q-py-xs q-my-none">
          <li>
            <q-checkbox
              label="Manually Load Stations Data"
              v-model="appstateStore.manualLoadStations"
            />
          </li>
        </ul>
      </DesignNote>
    </div>
  </q-page>
</template>

<script>
import { useAppstateStore } from "../stores/appstateStore";
import DesignNote from "components/DesignNote.vue";

export default {
  name: "IndexPage",
  setup() {
    const appstateStore = useAppstateStore();
    return { appstateStore };
  },
  data() {
    return {
      myName: "Home",
    };
  },
  computed: {},
  components: {
    DesignNote,
  },
  mounted() {
    this.appstateStore.currentSection = this.myName;
  },
};
</script>
