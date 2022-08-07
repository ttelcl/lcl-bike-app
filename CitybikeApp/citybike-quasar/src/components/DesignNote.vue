<template>
  <div class="design-note-out">
    <div :class="alt ? 'design-note-in-alt' : 'design-note-in'">
      <q-expansion-item
        :label="title"
        :icon="alt ? 'tune' : 'info_outline'"
        v-model="isExpanded"
      >
        <div class="q-px-xl q-pt-md">
          <slot> No content for this design note provided. </slot>
        </div>
      </q-expansion-item>
    </div>
  </div>
</template>

<script>
export default {
  name: "DesignNote",
  props: {
    // Remember to pass as ':expand="true"' because 'expand="true"' would pass a string!
    expand: {
      type: Boolean,
      default: false,
    },
    title: {
      type: String,
      default: "Design Note",
    },
    alt: {
      // alternate styling
      type: Boolean,
      default: false,
    },
  },
  data() {
    return {
      isExpanded: this.expand,
    };
  },
};
</script>

<style>
.design-note-out {
  padding: 1em 5em 0 5em;
}
.design-note-in {
  max-width: 40em;
  background-color: #113811;
  color: #ccddcc;
  font-style: italic;
  font-size: inherit;
  padding: 0.1em 0em 0.1em 0em;
  border-radius: 25px;
  border: 2px solid #ccddcc;
}
.design-note-in-alt {
  max-width: 40em;
  background-color: #585040;
  color: #ddddcc;
  font-style: italic;
  font-size: inherit;
  padding: 0.1em 0em 0.1em 0em;
  border-radius: 25px;
  border: 2px solid #ddddcc;
}
</style>
