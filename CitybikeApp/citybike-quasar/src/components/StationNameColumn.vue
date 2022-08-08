<!-- A specialized component for showing the name column in The Station Index table. Reused for each of the three languages -->
<template>
  <div class="row">
    <div class="col">
      <router-link :to="`/stations/${row.id}`" class="text-green-2">
        {{ row[nameField] }}
      </router-link>
    </div>
    <div class="col-auto">
      <q-btn
        icon="logout"
        :to="`/rides?dep=${row.id}`"
        color="primary"
        dense
        size="xs"
        class="q-mx-xs q-px-xs"
      >
        <q-tooltip :delay="500" class="text-body2">
          Show rides starting here
        </q-tooltip>
      </q-btn>
      <q-btn
        icon="login"
        :to="`/rides?ret=${row.id}`"
        color="primary"
        dense
        size="xs"
        class="q-px-xs"
      >
        <q-tooltip :delay="500" class="text-body2">
          Show rides ending here
        </q-tooltip>
      </q-btn>
    </div>
  </div>
</template>

<script>
export default {
  name: "StationNameColumn",
  props: {
    row: {
      type: Object,
      required: true,
    },
    nameField: {
      type: String,
      default: "nameFi",
    },
  },
  data() {
    return {
      // There is no local state!
    };
  },
};
</script>
