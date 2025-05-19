<template>
  <TreeSelect :options="materials" :multiple="multiple" :normalizer="normalizer" :modelValue="modelValue" :name="name"
    :required="required" :append-to-body="appendToBody" @update:modelValue="emit('update:modelValue', $event)"
    zIndex="3000" :disableFuzzyMatching="true">
    <template #option-label="{ node }" :class="labelClassName">
      <span>{{ node.label }}</span>
      <span v-if="node.raw.nhasanxuat" class="ml-1"><b>(NSX: {{ node.raw.nhasanxuat.tennsx }})</b></span>
    </template>
  </TreeSelect>
</template>

<script setup>
import { LOAD_ROOT_OPTIONS, ASYNC_SEARCH } from 'vue3-acies-treeselect'
import { storeToRefs } from "pinia";
import { computed, onMounted, ref } from "vue";

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
    default: "material",
  },
  useID: {
    type: Boolean,
    default: false,
  },
});
const emit = defineEmits(["update:modelValue"]);
const store = useGeneral();
const { materials } = storeToRefs(store);
const normalizer = (node) => {
  // var id = props.useID ? node.id : "m-" + node.id;
  return {
    id: node.mahh,
    label: node.mahh + " - " + node.tenhh,
  };
};
// const options = ref();
// const loadOptions = ({ action, callback }) => {
//   // console.log(action);
//   if (action === LOAD_ROOT_OPTIONS) {
//     let clone = JSON.parse(JSON.stringify(materials.value));
//     options.value = clone;
//     callback(null, options)
//   } else if (action === ASYNC_SEARCH) {
//     let clone = JSON.parse(JSON.stringify(materials.value));
//     options.value = clone;
//     callback(null, options)
//   }
// };
onMounted(async () => {
  await store.fetchMaterials();
  // let clone = JSON.parse(JSON.stringify(materials.value));
  // options.value = clone;

});
</script>
