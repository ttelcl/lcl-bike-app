<template>
  <div class="design-note-out">
    <div class="design-note-in">
      <q-expansion-item label="Design note" v-model="isExpanded">
        <div>
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
  },
  data() {
    return {
      isExpanded: !!this.expand,
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
  padding: 0.1em 2em 0.1em 2em;
  border-radius: 25px;
  border: 2px solid #ccddcc;
}
</style>
