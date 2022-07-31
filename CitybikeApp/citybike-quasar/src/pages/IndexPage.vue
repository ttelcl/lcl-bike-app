<template>
  <q-page class="q-pa-md">
    <q-breadcrumbs>
      <q-breadcrumbs-el label="Home" icon="home" />
    </q-breadcrumbs>
    <DesignNote :expand="true">
      <p>This page is the home / landing page of the SPA. Work in progress!</p>
      <p>
        Boxes like this present design notes (and are not part of the actual
        application).
      </p>
    </DesignNote>
    <div class="simple-text">
      <h2>Home</h2>
      <p>Home, sweet home</p>
      <h4>Temporary spot for backend service links</h4>
      <p>
        For these to work, the backend must be reachable. During development,
        that means both frontend server (http://localhost:9000/) and backend
        server (https://localhost:7185/) must be running
      </p>
      <ul>
        <li>
          <a href="/swagger/index.html">Swagger (backend Web API docs)</a>
        </li>
        <li>
          <a href="/api/scratch/dummy"
            >Direct API call example (without hitting DB)</a
          >
        </li>
      </ul>
    </div>
  </q-page>
</template>

<script>
import { mapWritableState } from "pinia";
import { useAppstateStore } from "../stores/appstateStore";
import DesignNote from "components/DesignNote.vue";

export default {
  name: "IndexPage",
  data() {
    return {
      myName: "Home",
    };
  },
  computed: {
    ...mapWritableState(useAppstateStore, ["currentSection"]),
  },
  components: {
    DesignNote,
  },
  mounted() {
    this.currentSection = this.myName;
  },
};
</script>
