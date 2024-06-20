<template>
  <TreeSelect
    :options="DutruChitiet"
    :multiple="multiple"
    :normalizer="normalizer"
    :modelValue="modelValue"
    :name="name"
    :required="required"
    :append-to-body="appendToBody"
    @update:modelValue="emit('update:modelValue', $event)"
    zIndex="3000"
    :disableFuzzyMatching="true"
  >
  </TreeSelect>
</template>

<script setup>
import { storeToRefs } from "pinia";
import { computed, onMounted } from "vue";

import { useGeneral } from "../../stores/general";
const props = defineProps({
  appendToBody: {
    type: Boolean,
    default: true,
  },
  modelValue: {
    type: [String, Array],
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
    default: "dutru_chitiet",
  },
});
const emit = defineEmits(["update:modelValue"]);
const store = useGeneral();
const { DutruChitiet } = storeToRefs(store);
const normalizer = (node) => {
  return {
    id: node.id,
    label: node.mahh + " - " + node.tenhh,
  };
};
onMounted(() => {
  store.fetchDutruChitiet();
});
</script>
