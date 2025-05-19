<template>
  <TreeSelect :options="supliers" :multiple="multiple" :normalizer="normalizer" :modelValue="modelValue" :name="name"
    :required="required" :append-to-body="appendToBody" @update:modelValue="emit('update:modelValue', $event)"
    zIndex="3000" :disableFuzzyMatching="true">
  </TreeSelect>
</template>
<script setup>
// import the component
import { storeToRefs } from 'pinia'
import { onMounted, computed, ref } from 'vue';

import { useGeneral } from '../../stores/general';

const store = useGeneral();
const { supliers } = storeToRefs(store)
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
    default: "supplier",
  },
  useID: {
    type: Boolean,
    default: true,
  }
});
const emit = defineEmits(["update:modelValue"]);

const normalizer = (node) => {
  // console.log(node);
  var id = props.useID ? node.id : node.mancc;
  return {
    id: id,
    label: node.mancc + " - " + node.tenncc,
  }
}
onMounted(() => {
  store.fetchNhacc();
});
</script>