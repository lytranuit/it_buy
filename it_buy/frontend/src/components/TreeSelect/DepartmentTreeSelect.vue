<template>
  <TreeSelect :options="departments" :multiple="multiple" :modelValue="modelValue" :flat="flat" :name="name"
    :required="required" :value-consists-of="valueConsistsOf" :append-to-body="true"
    @update:modelValue="emit('update:modelValue', $event)" :disabled="disabled" :clearable="clearable"></TreeSelect>
</template>

<script setup>
// import TreeSelect from "vue3-acies-treeselect";
import { useAuth } from "../../stores/auth";
import { storeToRefs } from "pinia";
import { computed, onMounted } from "vue";
const props = defineProps({
  modelValue: {
    type: [Number, Array],
  },
  valueConsistsOf: String, //ALL_WITH_INDETERMINATE
  multiple: {
    type: Boolean,
    default: false,
  },
  disabled: {
    type: Boolean,
    default: false,
  },
  clearable: {
    type: Boolean,
    default: true,
  },
  required: {
    type: Boolean,
    default: false,
  },
  name: {
    type: String,
    default: "dep",
  },
  flat: {
    type: Boolean,
    default: true,
  },
});
const emit = defineEmits(["update:modelValue"]);

const store = useAuth();
const { departments } = storeToRefs(store);
onMounted(() => {
  store.fetchDepartments();
});
</script>
