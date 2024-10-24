<template>
  <TreeSelect
    :options="products"
    :multiple="multiple"
    :normalizer="normalizer"
    :modelValue="modelValue"
    :name="name"
    :required="required"
    :append-to-body="appendToBody"
    placeholder="Chọn sản phẩm"
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
const { products } = storeToRefs(store);
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
  var id = node.mahh;
  // console.log(id);
  return {
    id: id,
    label: node.mahh + " - " + node.tenhh,
  };
};
onMounted(() => {
  store.fetchProducts();
});
</script>