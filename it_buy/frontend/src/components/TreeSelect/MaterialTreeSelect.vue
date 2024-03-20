<template>
  <TreeSelect :options="materials" :multiple="multiple" :normalizer="normalizer" :modelValue="modelValue" :name="name"
    :required="required" :append-to-body="appendToBody" @update:modelValue="emit('update:modelValue', $event)"
    zIndex="3000" :disableFuzzyMatching="true">
  </TreeSelect>
</template>

<script setup>
import { storeToRefs } from "pinia";
import { computed, onMounted } from "vue";

import { useGeneral } from '../../stores/general'
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
    default: "material",
  },
  useID: {
    type: Boolean,
    default: false,
  }
});
const emit = defineEmits(["update:modelValue"]);
const store = useGeneral();
const { materials } = storeToRefs(store);
const normalizer = (node) => {
  var id = props.useID ? node.id : "m-" + node.id;
  return {
    id: id,
    label: node.mahh + " - " + node.tenhh,
  }
}
onMounted(() => {
  store.fetchMaterials();
});
</script>
