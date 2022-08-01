<template>
  <q-item clickable :to="target" exact>
    <q-item-section v-if="icon" avatar>
      <q-icon :name="icon" />
    </q-item-section>
    <q-item-section>
      <q-item-label>{{ label }}</q-item-label>
    </q-item-section>
  </q-item>
</template>

<script>
export default {
  name: "AppSectionHeader",
  props: {
    icon: String,
    label: {
      type: String,
      required: true,
    },
    target: {
      type: String,
      required: true,
    },
  },
  data() {
    return {};
  },
};
</script>
