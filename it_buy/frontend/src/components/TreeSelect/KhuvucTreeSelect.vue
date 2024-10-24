<template>
  <TreeSelect
    :options="khuvuc"
    :multiple="multiple"
    :normalizer="normalizer"
    :modelValue="modelValue"
    :name="name"
    :required="required"
    :append-to-body="appendToBody"
    placeholder="Chọn khu vực"
    @update:modelValue="emit('update:modelValue', $event)"
    zIndex="3000"
    :disableFuzzyMatching="true"
  >
  </TreeSelect>
</template>
<script setup>
// import the component
import { storeToRefs } from "pinia";
import { onMounted, computed, ref } from "vue";

import { useGeneral } from "../../stores/general";

const store = useGeneral();
const { khuvuc } = storeToRefs(store);
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
    default: "khuvuc",
  },
});
const emit = defineEmits(["update:modelValue"]);

const normalizer = (node) => {
  var id = node.makhuvuc;
  // console.log(id);
  return {
    id: id,
    label: node.makhuvuc + " - " + node.tenkhuvuc,
  };
};
onMounted(() => {
  store.fetchKhuvuc();
});
</script>