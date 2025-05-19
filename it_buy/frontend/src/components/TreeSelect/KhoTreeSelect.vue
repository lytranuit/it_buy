<template>
  <TreeSelect :options="list_kho" :multiple="multiple" :normalizer="normalizer" :modelValue="modelValue" :name="name"
    :required="required" :append-to-body="appendToBody" placeholder="Chọn khu vực"
    @update:modelValue="emit('update:modelValue', $event)" zIndex="3000" :disableFuzzyMatching="true">
  </TreeSelect>
</template>
<script setup>
// import the component
import { storeToRefs } from "pinia";
import { onMounted, computed, ref } from "vue";

import { useGeneral } from "../../stores/general";
import { useAuth } from "../../stores/auth";
const store_auth = useAuth();
const store = useGeneral();
const { kho } = storeToRefs(store);
const { user } = storeToRefs(store_auth);
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
    default: "kho",
  },

  only_access: { type: Boolean, default: false },
});
const emit = defineEmits(["update:modelValue"]);

const list_kho = computed(() => {
  // console.log(kho.value);

  var clonedSelected = JSON.parse(JSON.stringify(kho.value));
  return clonedSelected.map((item) => {

    if (props.only_access) {
      // console.log(user.value.warehouses_vt);
      let khoVT = user.value.warehouses_vt.map((item) => item);
      // console.log(khoTP);
      item.isDisabled = khoVT.indexOf(item.makho) == -1;
    }
    return item;
  });
});
const normalizer = (node) => {
  return {
    id: node.makho,
    label: node.makho + " - " + node.tenkho,
    // isDisabled: node.disabled,
  };
};
onMounted(() => {
  store.fetchKho();
});
</script>