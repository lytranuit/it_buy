<template>
  <TreeSelect :options="PLHH" :multiple="multiple" :normalizer="normalizer" :modelValue="modelValue" :name="name"
    :required="required" :append-to-body="appendToBody" placeholder="Loại hàng hóa"
    @update:modelValue="emit('update:modelValue', $event)" zIndex="3000" :disableFuzzyMatching="true">
  </TreeSelect>
</template>
<script setup>
// import the component
import { storeToRefs } from "pinia";
import { onMounted, computed, ref } from "vue";

import { useGeneral } from "../../stores/general";

const store = useGeneral();
const { PLHH } = storeToRefs(store);
const props = defineProps({
  appendToBody: {
    type: Boolean,
    default: true,
  },
  modelValue: {
    type: [String, Array, Number],
  },
  multiple: {
    type: Boolean,
    default: false,
  },
  required: {
    type: Boolean,
    default: false,
  },
  name: {
    type: String,
    default: "PLHH",
  },
});
const emit = defineEmits(["update:modelValue"]);

const normalizer = (node) => {

  // console.log(id);
  return {
    id: node.pl,
    label: node.pl + " - " + node.tenpl,
  };
};
onMounted(() => {
  store.fetchPLHH();
});
</script>